namespace Redola.ActorModel
{
    public class ActorConfiguration
    {
        public const string ActorCenterTypeKey = @"ActorCenterType";
        public const string ActorCenterNameKey = @"ActorCenterName";
        public const string ActorCenterAddressKey = @"ActorCenterAddress";
        public const string ActorCenterPortKey = @"ActorCenterPort";

        public const string ActorTypeKey = @"ActorType";
        public const string ActorNameKey = @"ActorName";
        public const string ActorAddressKey = @"ActorAddress";
        public const string ActorPortKey = @"ActorPort";

        private ActorConfigurationReader _configReader = ActorConfigurationReader.Singleton();
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
            var actorType = _configReader.GetItem<string>(ActorCenterTypeKey);
            var actorName = _configReader.GetItem<string>(ActorCenterNameKey);
            var actorAddress = _configReader.GetItem<string>(ActorCenterAddressKey);
            var actorPort = _configReader.GetItem<string>(ActorCenterPortKey);

            var actor = new ActorDescription(actorType, actorName);
            actor.Address = actorAddress;
            actor.Port = actorPort;

            return actor;
        }

        private ActorDescription BuildLocalActor()
        {
            var actorType = _configReader.GetItem<string>(ActorTypeKey);
            var actorName = _configReader.GetItem<string>(ActorNameKey);
            var actorAddress = _configReader.GetItem<string>(ActorAddressKey);
            var actorPort = _configReader.GetItem<string>(ActorPortKey);

            var actor = new ActorDescription(actorType, actorName);
            actor.Address = actorAddress;
            actor.Port = actorPort;

            return actor;
        }
    }
}
