using System;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace Redola.ActorModel
{
    public class ActorConfigurationReader
    {
        private static ActorConfigurationReader _instance = new ActorConfigurationReader();

        public static ActorConfigurationReader Singleton()
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
