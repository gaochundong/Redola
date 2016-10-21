namespace Redola.ActorModel
{
    public class ActorConfiguration
    {
        private ActorSettingItems _configReader = ActorSettingItems.Singleton();
        private ActorIdentity _centerActor;
        private ActorIdentity _localActor;
        private ActorChannelConfiguration _channelConfiguration;

        public ActorConfiguration()
        {
        }

        public ActorIdentity CenterActor { get { return _centerActor; } }
        public ActorIdentity LocalActor { get { return _localActor; } }
        public ActorChannelConfiguration ChannelConfiguration { get { return _channelConfiguration; } }

        public virtual void Build()
        {
            _centerActor = BuildCenterActor();
            _localActor = BuildLocalActor();
            _channelConfiguration = new ActorChannelConfiguration();
        }

        private ActorIdentity BuildCenterActor()
        {
            var actorType = _configReader.GetItem<string>(ActorSettingItems.ActorCenterTypeKey);
            var actorName = _configReader.GetItem<string>(ActorSettingItems.ActorCenterNameKey);
            var actorAddress = _configReader.GetItem<string>(ActorSettingItems.ActorCenterAddressKey);
            var actorPort = _configReader.GetItem<string>(ActorSettingItems.ActorCenterPortKey);

            var actor = new ActorIdentity(actorType, actorName);
            actor.Address = actorAddress;
            actor.Port = actorPort;

            return actor;
        }

        private ActorIdentity BuildLocalActor()
        {
            var actorType = _configReader.GetItem<string>(ActorSettingItems.ActorTypeKey);
            var actorName = _configReader.GetItem<string>(ActorSettingItems.ActorNameKey);
            var actorAddress = _configReader.GetItem<string>(ActorSettingItems.ActorAddressKey);
            var actorPort = _configReader.GetItem<string>(ActorSettingItems.ActorPortKey);

            var actor = new ActorIdentity(actorType, actorName);
            actor.Address = actorAddress;
            actor.Port = actorPort;

            return actor;
        }
    }
}
