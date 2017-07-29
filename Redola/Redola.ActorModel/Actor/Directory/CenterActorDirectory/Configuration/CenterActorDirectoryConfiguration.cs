namespace Redola.ActorModel
{
    public abstract class CenterActorDirectoryConfiguration
    {
        private ActorIdentity _centerActor;
        private ActorChannelConfiguration _channelConfiguration;

        public CenterActorDirectoryConfiguration()
        {
        }

        public ActorIdentity CenterActor { get { return _centerActor; } }
        public ActorChannelConfiguration ChannelConfiguration { get { return _channelConfiguration; } }

        public CenterActorDirectoryConfiguration Build()
        {
            _centerActor = BuildCenterActor();
            _channelConfiguration = BuildChannelConfiguration();

            return this;
        }

        protected abstract ActorIdentity BuildCenterActor();

        protected virtual ActorChannelConfiguration BuildChannelConfiguration()
        {
            return new ActorChannelConfiguration();
        }
    }
}
