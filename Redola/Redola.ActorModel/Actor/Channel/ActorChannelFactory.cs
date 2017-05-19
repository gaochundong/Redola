using System;
using System.Net;
using Logrila.Logging;

namespace Redola.ActorModel
{
    public class ActorChannelFactory
    {
        private ILog _log = Logger.Get<ActorChannelFactory>();
        private IActorDirectory _directory;
        private ActorChannelConfiguration _channelConfiguration;

        public ActorChannelFactory(
            IActorDirectory directory,
            ActorChannelConfiguration channelConfiguration)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (channelConfiguration == null)
                throw new ArgumentNullException("channelConfiguration");

            _directory = directory;
            _channelConfiguration = channelConfiguration;
        }

        public IActorChannel BuildLocalActor(ActorIdentity localActor)
        {
            if (string.IsNullOrWhiteSpace(localActor.Address))
                throw new InvalidOperationException(string.Format(
                    "Invalid local actor address, [{0}].", localActor));

            int localPort = -1;
            if (!int.TryParse(localActor.Port, out localPort) || localPort < 0)
                throw new InvalidOperationException(string.Format(
                    "Invalid local actor port, [{0}].", localActor));

            var localActorEndPoint = new IPEndPoint(IPAddress.Any, localPort);
            var channel = new ActorListenerChannel(
                localActor,
                new ActorTransportListener(localActorEndPoint, _channelConfiguration.TransportConfiguration),
                _channelConfiguration);

            return channel;
        }

        public IActorChannel BuildActorChannel(ActorIdentity localActor, string remoteActorType, string remoteActorName)
        {
            var remoteActorEndPoint = _directory.LookupRemoteActorEndPoint(remoteActorType, remoteActorName);
            if (remoteActorEndPoint == null)
                throw new ActorNotFoundException(string.Format(
                    "Cannot find remote actor endpoint, Type[{0}], Name[{1}].", remoteActorType, remoteActorName));

            var channel = new ActorConnectorChannel(
                localActor,
                new ActorTransportConnector(remoteActorEndPoint, _channelConfiguration.TransportConfiguration),
                _channelConfiguration);

            return channel;
        }

        public IActorChannel BuildActorChannel(ActorIdentity localActor, string remoteActorType)
        {
            var remoteActorEndPoint = _directory.LookupRemoteActorEndPoint(remoteActorType);
            if (remoteActorEndPoint == null)
                throw new ActorNotFoundException(string.Format(
                    "Cannot find remote actor endpoint, Type[{0}].", remoteActorType));

            var channel = new ActorConnectorChannel(
                localActor,
                new ActorTransportConnector(remoteActorEndPoint, _channelConfiguration.TransportConfiguration),
                _channelConfiguration);

            return channel;
        }
    }
}
