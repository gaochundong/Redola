using System;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace Redola.ActorModel
{
    public class AppConfigCenterActorDirectorySettingItems
    {
        public const string CenterActorTypeKey = @"CenterActorType";
        public const string CenterActorNameKey = @"CenterActorName";
        public const string CenterActorAddressKey = @"CenterActorAddress";
        public const string CenterActorPortKey = @"CenterActorPort";

        public const string KeepAliveIntervalKey = @"KeepAliveIntervalByMilliseconds";
        public const string KeepAliveTimeoutKey = @"KeepAliveTimeoutByMilliseconds";
        public const string KeepAliveEnabledKey = @"KeepAliveEnabled";

        private static AppConfigCenterActorDirectorySettingItems _instance = new AppConfigCenterActorDirectorySettingItems();

        public static AppConfigCenterActorDirectorySettingItems Singleton()
        {
            return _instance;
        }

        public T GetItem<T>(string itemName) where T : IConvertible
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(itemName))
            {
                return (T)Convert.ChangeType(
                    ConfigurationManager.AppSettings[itemName],
                    typeof(T), CultureInfo.InvariantCulture);
            }

            return default(T);
        }

        public bool ContainsItem(string itemName)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(itemName);
        }
    }
}
