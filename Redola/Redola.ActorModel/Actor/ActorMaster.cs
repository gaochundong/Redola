using System.Collections.Generic;
using System.Linq;
using Logrila.Logging;
using Redola.ActorModel.Framing;

namespace Redola.ActorModel
{
    public class ActorMaster : Actor
    {
        private ILog _log = Logger.Get<ActorMaster>();

        public ActorMaster()
            : this(AppConfigActorConfiguration.Load())
        {
        }

        public ActorMaster(ActorConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void OnActorDataReceived(object sender, ActorDataReceivedEventArgs e)
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

                _log.DebugFormat("Lookup actors [{0}], RemoteActor[{1}].", actorCollection.Items.Count, e.RemoteActor);
                this.BeginSend(e.RemoteActor.Type, e.RemoteActor.Name, actorLookupRequestBuffer);
            }
            else
            {
                base.OnActorDataReceived(sender, e);
            }
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
