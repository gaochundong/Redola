using System;
using System.Collections.Generic;
using System.IO;

namespace Redola.Rpc.ServiceDiscovery.XmlIntegration
{
    public class LocalXmlFileServiceRegistry
    {
        private string _localXmlFilePath = string.Empty;
        private XmlServiceRegistry _registry;

        public LocalXmlFileServiceRegistry(string localXmlFilePath)
        {
            if (string.IsNullOrEmpty(localXmlFilePath))
                throw new ArgumentNullException("localXmlFilePath");
            if (!File.Exists(localXmlFilePath))
                throw new FileNotFoundException("Cannot find the xml service registry file.", localXmlFilePath);
            _localXmlFilePath = localXmlFilePath;

            var fileContent = File.ReadAllText(_localXmlFilePath);
            _registry = XmlConvert.DeserializeObject<XmlServiceRegistry>(fileContent);
        }

        public string LocalXmlFilePath
        {
            get { return _localXmlFilePath; }
        }

        public IEnumerable<XmlServiceRegistryEntry> Entries
        {
            get { return _registry.Entries; }
        }

        public static LocalXmlFileServiceRegistry Load(string localXmlFilePath)
        {
            var configuration = new LocalXmlFileServiceRegistry(localXmlFilePath);
            return configuration;
        }
    }
}
