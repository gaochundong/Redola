using System;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace Redola.ActorModel
{
    public class AppConfigActorSettingItems
    {
        public const string ActorCenterTypeKey = @"ActorCenterType";
        public const string ActorCenterNameKey = @"ActorCenterName";
        public const string ActorCenterAddressKey = @"ActorCenterAddress";
        public const string ActorCenterPortKey = @"ActorCenterPort";

        public const string ActorTypeKey = @"ActorType";
        public const string ActorNameKey = @"ActorName";
        public const string ActorAddressKey = @"ActorAddress";
        public const string ActorPortKey = @"ActorPort";

        private static AppConfigActorSettingItems _instance = new AppConfigActorSettingItems();

        public static AppConfigActorSettingItems Singleton()
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
    }
}
