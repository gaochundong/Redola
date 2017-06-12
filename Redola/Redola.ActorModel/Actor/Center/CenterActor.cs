using System.Collections.Generic;
using System.Linq;
using Logrila.Logging;
using Redola.ActorModel.Framing;

namespace Redola.ActorModel
{
    public class CenterActor : Actor
    {
        private ILog _log = Logger.Get<CenterActor>();

        public CenterActor()
            : this(AppConfigActorConfiguration.Load())
        {
        }

        public CenterActor(ActorConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void OnActorChannelConnected(object sender, ActorChannelConnectedEventArgs e)
        {
            base.OnActorChannelConnected(sender, e);
            NotifyActorChanged(e.RemoteActor);
        }

        protected override void OnActorChannelDisconnected(object sender, ActorChannelDisconnectedEventArgs e)
        {
            base.OnActorChannelDisconnected(sender, e);
            NotifyActorChanged(e.RemoteActor);
        }

        protected override void OnActorChannelDataReceived(object sender, ActorChannelDataReceivedEventArgs e)
        {
            ActorFrameHeader actorLookupRequestFrameHeader = null;
            bool isHeaderDecoded = this.ChannelConfiguration.FrameBuilder.TryDecodeFrameHeader(
                e.Data, e.DataOffset, e.DataLength,
                out actorLookupRequestFrameHeader);
            if (isHeaderDecoded && actorLookupRequestFrameHeader.OpCode == OpCode.Where)
            {
                byte[] payload;
                int payloadOffset;
                int payloadCount;
                this.ChannelConfiguration.FrameBuilder.DecodePayload(
                    e.Data, e.DataOffset, actorLookupRequestFrameHeader,
                    out payload, out payloadOffset, out payloadCount);
                var actorLookupRequestData = this.ChannelConfiguration
                    .FrameBuilder
                    .ControlFrameDataDecoder
                    .DecodeFrameData<ActorIdentityLookup>(payload, payloadOffset, payloadCount);
                var lookupActorType = actorLookupRequestData != null ? actorLookupRequestData.Type : null;

                var actorCollection = new ActorIdentityCollection();
                actorCollection.Items.AddRange(this.GetAllActors().Where(a => a.Type == lookupActorType).ToList());
                var actorLookupResponseData = this.ChannelConfiguration.FrameBuilder.ControlFrameDataEncoder.EncodeFrameData(actorCollection);
                var actorLookupResponse = new HereFrame(actorLookupResponseData);
                var actorLookupRequestBuffer = this.ChannelConfiguration.FrameBuilder.EncodeFrame(actorLookupResponse);

                _log.DebugFormat("Lookup actors, ActorType[{0}], Count[{1}], RemoteActor[{2}].",
                    lookupActorType, actorCollection.Items.Count, e.RemoteActor);
                this.Send(e.RemoteActor, actorLookupRequestBuffer);
            }
            else
            {
                base.OnActorChannelDataReceived(sender, e);
            }
        }

        private void NotifyActorChanged(ActorIdentity changedActor)
        {
            var actorCollection = new ActorIdentityCollection();
            actorCollection.Items.AddRange(this.GetAllActors().Where(a => a.Type == changedActor.Type).ToList());
            var actorChangedNotificationData = this.ChannelConfiguration.FrameBuilder.ControlFrameDataEncoder.EncodeFrameData(actorCollection);
            var actorChangedNotification = new HereFrame(actorChangedNotificationData);
            var actorChangedNotificationBuffer = this.ChannelConfiguration.FrameBuilder.EncodeFrame(actorChangedNotification);

            _log.DebugFormat("Broadcast actors changed, ActorType[{0}], Count[{1}].",
                changedActor.Type, actorCollection.Items.Count);
            this.BeginBroadcast(actorChangedNotificationBuffer);
        }

        public new IEnumerable<ActorIdentity> GetAllActors()
        {
            return base.GetAllActors();
        }

        public IEnumerable<ActorIdentity> GetAllActors(string actorType)
        {
            return this.GetAllActors().Where(a => a.Type == actorType);
        }
    }
}
