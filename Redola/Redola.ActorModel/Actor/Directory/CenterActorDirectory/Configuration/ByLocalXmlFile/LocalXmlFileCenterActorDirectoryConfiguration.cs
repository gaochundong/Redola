using System;
using System.Collections.Generic;
using System.IO;
using Redola.ActorModel.Serialization;

namespace Redola.ActorModel
{
    public class LocalXmlFileCenterActorDirectoryConfiguration : CenterActorDirectoryConfiguration
    {
        private string _localXmlFilePath = string.Empty;
        private XmlCenterActorDirectoryConfiguration _configuration;

        private static readonly ActorIdentity NoCenterActor
            = new ActorIdentity()
            {
                Type = "type-of-no-center-actor",
                Name = "name-of-no-center-actor",
                Address = "localhost",
                Port = "0",
            };

        public LocalXmlFileCenterActorDirectoryConfiguration(string localXmlFilePath)
        {
            if (string.IsNullOrEmpty(localXmlFilePath))
                throw new ArgumentNullException("localXmlFilePath");
            if (!File.Exists(localXmlFilePath))
                throw new FileNotFoundException("Cannot find the xml center actor directory configuration file.", localXmlFilePath);
            _localXmlFilePath = localXmlFilePath;

            var fileContent = File.ReadAllText(_localXmlFilePath);
            _configuration = XmlConvert.DeserializeObject<XmlCenterActorDirectoryConfiguration>(fileContent);
        }

        public string LocalXmlFilePath
        {
            get { return _localXmlFilePath; }
        }

        public IEnumerable<ActorIdentity> ActorDirectory
        {
            get { return _configuration.Directory; }
        }

        protected override ActorIdentity BuildCenterActor()
        {
            if (_configuration.CenterActor == null)
                return NoCenterActor;

            var actorType = _configuration.CenterActor.Type;
            var actorName = _configuration.CenterActor.Name;
            var actorAddress = _configuration.CenterActor.Address;
            var actorPort = _configuration.CenterActor.Port;
            if (string.IsNullOrWhiteSpace(actorType))
                throw new InvalidProgramException(
                    string.Format("Center actor type cannot be empty."));
            if (string.IsNullOrWhiteSpace(actorName))
                throw new InvalidProgramException(
                    string.Format("Center actor name cannot be empty."));
            if (string.IsNullOrWhiteSpace(actorAddress))
                throw new InvalidProgramException(
                    string.Format("Center actor address cannot be empty."));
            if (string.IsNullOrWhiteSpace(actorPort))
                throw new InvalidProgramException(
                    string.Format("Center actor port cannot be empty."));

            var actor = new ActorIdentity(actorType, actorName);
            actor.Address = actorAddress;
            actor.Port = actorPort;

            return actor;
        }

        public static LocalXmlFileCenterActorDirectoryConfiguration Load(string localXmlFilePath)
        {
            var configuration = new LocalXmlFileCenterActorDirectoryConfiguration(localXmlFilePath);
            configuration.Build();
            return configuration;
        }
    }
}
