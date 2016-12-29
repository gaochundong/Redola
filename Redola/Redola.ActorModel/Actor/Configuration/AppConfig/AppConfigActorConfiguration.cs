using System;

namespace Redola.ActorModel
{
    public class AppConfigActorConfiguration : ActorConfiguration
    {
        private AppConfigActorSettingItems _appConfig = AppConfigActorSettingItems.Singleton();

        public AppConfigActorConfiguration()
        {
        }

        protected override ActorIdentity BuildCenterActor()
        {
            var actorType = _appConfig.GetItem<string>(AppConfigActorSettingItems.ActorCenterTypeKey);
            var actorName = _appConfig.GetItem<string>(AppConfigActorSettingItems.ActorCenterNameKey);
            var actorAddress = _appConfig.GetItem<string>(AppConfigActorSettingItems.ActorCenterAddressKey);
            var actorPort = _appConfig.GetItem<string>(AppConfigActorSettingItems.ActorCenterPortKey);
            if (string.IsNullOrWhiteSpace(actorType))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.ActorCenterTypeKey));
            if (string.IsNullOrWhiteSpace(actorName))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.ActorCenterNameKey));
            if (string.IsNullOrWhiteSpace(actorAddress))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.ActorCenterAddressKey));
            if (string.IsNullOrWhiteSpace(actorPort))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.ActorCenterPortKey));

            var actor = new ActorIdentity(actorType, actorName);
            actor.Address = actorAddress;
            actor.Port = actorPort;

            return actor;
        }

        protected override ActorIdentity BuildLocalActor()
        {
            var actorType = _appConfig.GetItem<string>(AppConfigActorSettingItems.ActorTypeKey);
            var actorName = _appConfig.GetItem<string>(AppConfigActorSettingItems.ActorNameKey);
            var actorAddress = _appConfig.GetItem<string>(AppConfigActorSettingItems.ActorAddressKey);
            var actorPort = _appConfig.GetItem<string>(AppConfigActorSettingItems.ActorPortKey);
            if (string.IsNullOrWhiteSpace(actorType))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.ActorTypeKey));
            if (string.IsNullOrWhiteSpace(actorName))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.ActorNameKey));
            if (string.IsNullOrWhiteSpace(actorAddress))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.ActorAddressKey));
            if (string.IsNullOrWhiteSpace(actorPort))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.ActorPortKey));

            var actor = new ActorIdentity(actorType, actorName);
            actor.Address = actorAddress;
            actor.Port = actorPort;

            return actor;
        }

        public static AppConfigActorConfiguration Load()
        {
            var configuration = new AppConfigActorConfiguration();
            configuration.Build();
            return configuration;
        }
    }
}
