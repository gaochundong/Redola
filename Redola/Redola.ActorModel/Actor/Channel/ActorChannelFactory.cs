using System;
using System.Net;
using Logrila.Logging;
using Redola.ActorModel.Framing;

namespace Redola.ActorModel
{
    public class ActorChannelFactory
    {
        private ILog _log = Logger.Get<ActorChannelFactory>();
        private ActorDirectory _directory;
        private IActorFrameBuilder _frameBuilder;

        public ActorChannelFactory(
            ActorDirectory directory,
            IActorFrameBuilder frameBuilder)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (frameBuilder == null)
                throw new ArgumentNullException("frameBuilder");

            _directory = directory;
            _frameBuilder = frameBuilder;
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
            var channel = new ActorListenerChannel(
                localActor, 
                new ActorTransportListener(localEndPoint),
                _frameBuilder);

            return channel;
        }

        public IActorChannel BuildActorChannel(ActorDescription localActor, string remoteActorType, string remoteActorName)
        {
            var actorEndPoint = _directory.LookupRemoteActorEndPoint(remoteActorType, remoteActorName);
            if (actorEndPoint == null)
                throw new ActorNotFoundException(string.Format(
                    "Cannot find remote actor endpoint, Type[{0}], Name[{1}].", remoteActorType, remoteActorName));

            var channel = new ActorConnectorChannel(
                localActor, 
                new ActorTransportConnector(actorEndPoint),
                _frameBuilder);

            return channel;
        }

        public IActorChannel BuildActorChannel(ActorDescription localActor, string remoteActorType)
        {
            var actorEndPoint = _directory.LookupRemoteActorEndPoint(remoteActorType);
            if (actorEndPoint == null)
                throw new ActorNotFoundException(string.Format(
                    "Cannot find remote actor endpoint, Type[{0}].", remoteActorType));

            var channel = new ActorConnectorChannel(
                localActor, 
                new ActorTransportConnector(actorEndPoint),
                _frameBuilder);

            return channel;
        }
    }
}
