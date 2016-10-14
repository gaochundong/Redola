using System;
using System.Net;
using Logrila.Logging;

namespace Redola.ActorModel
{
    public class ActorChannelFactory
    {
        private ILog _log = Logger.Get<ActorChannelFactory>();
        private ActorDirectory _directory;
        private IActorMessageEncoder _encoder;
        private IActorMessageDecoder _decoder;

        public ActorChannelFactory(
            ActorDirectory directory, 
            IActorMessageEncoder encoder, IActorMessageDecoder decoder)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (encoder == null)
                throw new ArgumentNullException("encoder");
            if (decoder == null)
                throw new ArgumentNullException("decoder");

            _directory = directory;
            _encoder = encoder;
            _decoder = decoder;
        }

        public IActorChannel BuildLocalActor(ActorDescription localActor)
        {
            if (string.IsNullOrWhiteSpace(localActor.Address))
                throw new InvalidOperationException(string.Format(
                    "Invalid local actor address, [{0}].", localActor));

            int localPort = -1;
            if (!int.TryParse(localActor.Port, out localPort))
                throw new InvalidOperationException(string.Format(
                    "Invalid local actor port, [{0}].", localActor));

            var localEndPoint = new IPEndPoint(IPAddress.Any, localPort);
            var channel = new ActorListenerChannel(localActor, new ActorTransportListener(localEndPoint), _encoder, _decoder);

            return channel;
        }

        public IActorChannel BuildActorChannel(ActorDescription localActor, string remoteActorType, string remoteActorName)
        {
            var actorEndPoint = _directory.LookupRemoteActorEndPoint(remoteActorType, remoteActorName);
            if (actorEndPoint == null)
                throw new ActorNotFoundException(string.Format(
                    "Cannot find remote actor endpoint, Type[{0}], Name[{1}].", remoteActorType, remoteActorName));

            var channel = new ActorConnectorChannel(localActor, new ActorTransportConnector(actorEndPoint), _encoder, _decoder);

            return channel;
        }

        public IActorChannel BuildActorChannel(ActorDescription localActor, string remoteActorType)
        {
            var actorEndPoint = _directory.LookupRemoteActorEndPoint(remoteActorType);
            if (actorEndPoint == null)
                throw new ActorNotFoundException(string.Format(
                    "Cannot find remote actor endpoint, Type[{0}].", remoteActorType));

            var channel = new ActorConnectorChannel(localActor, new ActorTransportConnector(actorEndPoint), _encoder, _decoder);

            return channel;
        }
    }
}
