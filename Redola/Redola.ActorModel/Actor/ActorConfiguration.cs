using System;
using Redola.ActorModel.Framing;
using Redola.ActorModel.Serialization;

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
        private IActorFrameBuilder _frameBuilder;
        private ActorDescription _centerActor;
        private ActorDescription _localActor;

        public ActorConfiguration()
        {
        }

        public IActorFrameBuilder FrameBuilder { get { return _frameBuilder; } }
        public ActorDescription CenterActor { get { return _centerActor; } }
        public ActorDescription LocalActor { get { return _localActor; } }

        public void Build()
        {
            var messageEncoder = new XmlMessageEncoder();
            var messageDecoder = new XmlMessageDecoder();
            var controlFrameDataEncoder = new XmlActorControlFrameDataEncoder(messageEncoder);
            var controlFrameDataDecoder = new XmlActorControlFrameDataDecoder(messageDecoder);

            _frameBuilder = new ActorFrameBuilder(controlFrameDataEncoder, controlFrameDataDecoder);
            _centerActor = BuildCenterActor();
            _localActor = BuildLocalActor();
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
