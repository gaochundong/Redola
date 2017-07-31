using System;
using System.Collections.Generic;
using System.IO;
using Redola.ActorModel;

namespace Redola.Rpc.ServiceDiscovery.XmlIntegration
{
    public class LocalXmlFileActorRegistry
    {
        private string _localXmlFilePath = string.Empty;
        private XmlActorRegistry _registry;

        public LocalXmlFileActorRegistry(string localXmlFilePath)
        {
            if (string.IsNullOrEmpty(localXmlFilePath))
                throw new ArgumentNullException("localXmlFilePath");
            if (!File.Exists(localXmlFilePath))
                throw new FileNotFoundException("Cannot find the xml file.", localXmlFilePath);
            _localXmlFilePath = localXmlFilePath;

            var fileContent = File.ReadAllText(_localXmlFilePath);
            _registry = XmlConvert.DeserializeObject<XmlActorRegistry>(fileContent);
        }

        public string LocalXmlFilePath
        {
            get { return _localXmlFilePath; }
        }

        public IEnumerable<ActorIdentity> GetEntries()
        {
            return _registry.Entries;
        }

        public static LocalXmlFileActorRegistry Load(string localXmlFilePath)
        {
            var registry = new LocalXmlFileActorRegistry(localXmlFilePath);
            return registry;
        }
    }
}
