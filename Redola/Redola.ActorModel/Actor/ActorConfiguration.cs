namespace Redola.ActorModel
{
    public class ActorConfiguration
    {
        private ActorSettingItems _configReader = ActorSettingItems.Singleton();
        private ActorDescription _centerActor;
        private ActorDescription _localActor;
        private ActorChannelConfiguration _channelConfiguration;

        public ActorConfiguration()
        {
        }

        public ActorDescription CenterActor { get { return _centerActor; } }
        public ActorDescription LocalActor { get { return _localActor; } }
        public ActorChannelConfiguration ChannelConfiguration { get { return _channelConfiguration; } }

        public void Build()
        {
            _centerActor = BuildCenterActor();
            _localActor = BuildLocalActor();
            _channelConfiguration = new ActorChannelConfiguration();
        }

        private ActorDescription BuildCenterActor()
        {
            var actorType = _configReader.GetItem<string>(ActorSettingItems.ActorCenterTypeKey);
            var actorName = _configReader.GetItem<string>(ActorSettingItems.ActorCenterNameKey);
            var actorAddress = _configReader.GetItem<string>(ActorSettingItems.ActorCenterAddressKey);
            var actorPort = _configReader.GetItem<string>(ActorSettingItems.ActorCenterPortKey);

            var actor = new ActorDescription(actorType, actorName);
            actor.Address = actorAddress;
            actor.Port = actorPort;

            return actor;
        }

        private ActorDescription BuildLocalActor()
        {
            var actorType = _configReader.GetItem<string>(ActorSettingItems.ActorTypeKey);
            var actorName = _configReader.GetItem<string>(ActorSettingItems.ActorNameKey);
            var actorAddress = _configReader.GetItem<string>(ActorSettingItems.ActorAddressKey);
            var actorPort = _configReader.GetItem<string>(ActorSettingItems.ActorPortKey);

            var actor = new ActorDescription(actorType, actorName);
            actor.Address = actorAddress;
            actor.Port = actorPort;

            return actor;
        }
    }
}
