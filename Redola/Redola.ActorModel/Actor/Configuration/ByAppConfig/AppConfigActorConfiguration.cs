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
            var actorType = _appConfig.GetItem<string>(AppConfigActorSettingItems.CenterActorTypeKey);
            var actorName = _appConfig.GetItem<string>(AppConfigActorSettingItems.CenterActorNameKey);
            var actorAddress = _appConfig.GetItem<string>(AppConfigActorSettingItems.CenterActorAddressKey);
            var actorPort = _appConfig.GetItem<string>(AppConfigActorSettingItems.CenterActorPortKey);
            if (string.IsNullOrWhiteSpace(actorType))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.CenterActorTypeKey));
            if (string.IsNullOrWhiteSpace(actorName))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.CenterActorNameKey));
            if (string.IsNullOrWhiteSpace(actorAddress))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.CenterActorAddressKey));
            if (string.IsNullOrWhiteSpace(actorPort))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigActorSettingItems.CenterActorPortKey));

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

        protected override ActorChannelConfiguration BuildChannelConfiguration()
        {
            var configuration = new ActorChannelConfiguration();

            if (_appConfig.ContainsItem(AppConfigActorSettingItems.KeepAliveIntervalKey))
            {
                var keepAliveInterval = _appConfig.GetItem<int>(AppConfigActorSettingItems.KeepAliveIntervalKey);
                if (keepAliveInterval < 1)
                    throw new InvalidProgramException(
                        string.Format("Item [{0}] setting is invalid.", AppConfigActorSettingItems.KeepAliveIntervalKey));
                configuration.KeepAliveInterval = TimeSpan.FromMilliseconds(keepAliveInterval);
            }

            if (_appConfig.ContainsItem(AppConfigActorSettingItems.KeepAliveTimeoutKey))
            {
                var keepAliveTimeout = _appConfig.GetItem<int>(AppConfigActorSettingItems.KeepAliveTimeoutKey);
                if (keepAliveTimeout < 1)
                    throw new InvalidProgramException(
                        string.Format("Item [{0}] setting is invalid.", AppConfigActorSettingItems.KeepAliveTimeoutKey));
                configuration.KeepAliveTimeout = TimeSpan.FromMilliseconds(keepAliveTimeout);
            }

            if (_appConfig.ContainsItem(AppConfigActorSettingItems.KeepAliveEnabledKey))
            {
                var keepAliveEnabled = _appConfig.GetItem<bool>(AppConfigActorSettingItems.KeepAliveEnabledKey);
                configuration.KeepAliveEnabled = keepAliveEnabled;
            }

            return configuration;
        }

        public static AppConfigActorConfiguration Load()
        {
            var configuration = new AppConfigActorConfiguration();
            configuration.Build();
            return configuration;
        }
    }
}
