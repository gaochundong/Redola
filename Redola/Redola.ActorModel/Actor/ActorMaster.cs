using System.Collections.Generic;
using System.Linq;
using Logrila.Logging;

namespace Redola.ActorModel
{
    public class ActorMaster : Actor
    {
        private ILog _log = Logger.Get<ActorMaster>();

        public ActorMaster(ActorConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void OnActorDataReceived(object sender, ActorDataReceivedEventArgs e)
        {
            var requestMessage = this.Decoder.DecodeEnvelope(e.Data, e.DataOffset, e.DataLength);
            if (requestMessage.MessageType == typeof(ActorLookupRequest).Name)
            {
                var request = this.Decoder.DecodeMessage<ActorLookupRequest>(
                    requestMessage.MessageData, 0, requestMessage.MessageData.Length);
                if (request != null)
                {
                    var response = new ActorLookupResponse();
                    response.Actors = this.GetAllActors().ToList();
                    var responseBuffer = this.Encoder.EncodeMessageEnvelope(response);

                    _log.InfoFormat("Lookup actors [{0}], RemoteActor[{1}].", response.Actors.Count, e.RemoteActor);
                    this.BeginSend(e.RemoteActor.Type, e.RemoteActor.Name, responseBuffer);
                }
            }
            else
            {
                base.OnActorDataReceived(sender, e);
            }
        }

        public new IEnumerable<ActorDescription> GetAllActors()
        {
            return base.GetAllActors();
        }

        public IEnumerable<ActorDescription> GetAllActors(string actorType)
        {
            return this.GetAllActors().Where(a => a.Type == actorType);
        }
    }
}
