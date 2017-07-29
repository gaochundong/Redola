using System;
using System.Collections.Generic;
using System.IO;
using Redola.ActorModel.Serialization;

namespace Redola.ActorModel
{
    public class LocalXmlFileActorDirectoryConfiguration
    {
        private string _localXmlFilePath = string.Empty;
        private XmlLocalXmlFileActorDirectoryConfiguration _configuration;

        public LocalXmlFileActorDirectoryConfiguration(string localXmlFilePath)
        {
            if (string.IsNullOrEmpty(localXmlFilePath))
                throw new ArgumentNullException("localXmlFilePath");
            if (!File.Exists(localXmlFilePath))
                throw new FileNotFoundException("Cannot find the xml local file actor directory configuration file.", localXmlFilePath);
            _localXmlFilePath = localXmlFilePath;

            var fileContent = File.ReadAllText(_localXmlFilePath);
            _configuration = XmlConvert.DeserializeObject<XmlLocalXmlFileActorDirectoryConfiguration>(fileContent);
        }

        public string LocalXmlFilePath
        {
            get { return _localXmlFilePath; }
        }

        public IEnumerable<ActorIdentity> ActorDirectory
        {
            get { return _configuration.Directory; }
        }

        public static LocalXmlFileActorDirectoryConfiguration Load(string localXmlFilePath)
        {
            var configuration = new LocalXmlFileActorDirectoryConfiguration(localXmlFilePath);
            return configuration;
        }
    }
}
