using System;

namespace Redola.ActorModel
{
    public class AppConfigCenterActorDirectoryConfiguration : CenterActorDirectoryConfiguration
    {
        private AppConfigCenterActorDirectorySettingItems _appConfig = AppConfigCenterActorDirectorySettingItems.Singleton();

        public AppConfigCenterActorDirectoryConfiguration()
        {
        }

        protected override ActorIdentity BuildCenterActor()
        {
            var actorType = _appConfig.GetItem<string>(AppConfigCenterActorDirectorySettingItems.CenterActorTypeKey);
            var actorName = _appConfig.GetItem<string>(AppConfigCenterActorDirectorySettingItems.CenterActorNameKey);
            var actorAddress = _appConfig.GetItem<string>(AppConfigCenterActorDirectorySettingItems.CenterActorAddressKey);
            var actorPort = _appConfig.GetItem<string>(AppConfigCenterActorDirectorySettingItems.CenterActorPortKey);
            if (string.IsNullOrWhiteSpace(actorType))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigCenterActorDirectorySettingItems.CenterActorTypeKey));
            if (string.IsNullOrWhiteSpace(actorName))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigCenterActorDirectorySettingItems.CenterActorNameKey));
            if (string.IsNullOrWhiteSpace(actorAddress))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigCenterActorDirectorySettingItems.CenterActorAddressKey));
            if (string.IsNullOrWhiteSpace(actorPort))
                throw new InvalidProgramException(
                    string.Format("Item [{0}] setting cannot be empty.", AppConfigCenterActorDirectorySettingItems.CenterActorPortKey));

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

        public static AppConfigCenterActorDirectoryConfiguration Load()
        {
            var configuration = new AppConfigCenterActorDirectoryConfiguration();
            configuration.Build();
            return configuration;
        }
    }
}
