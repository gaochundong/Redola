using System;
using System.Collections.Generic;
using System.IO;
using Redola.ActorModel.Serialization;

namespace Redola.ActorModel
{
    public class LocalXmlFileActorConfiguration : ActorConfiguration
    {
        private string _localXmlFilePath = string.Empty;
        private XmlActorConfiguration _configuration;

        public LocalXmlFileActorConfiguration(string localXmlFilePath)
        {
            if (string.IsNullOrEmpty(localXmlFilePath))
                throw new ArgumentNullException("localXmlFilePath");
            if (!File.Exists(localXmlFilePath))
                throw new FileNotFoundException("Cannot find the xml actor configuration file.", localXmlFilePath);
            _localXmlFilePath = localXmlFilePath;

            var fileContent = File.ReadAllText(_localXmlFilePath);
            _configuration = XmlConvert.DeserializeObject<XmlActorConfiguration>(fileContent);
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

        protected override ActorIdentity BuildLocalActor()
        {
            var actorType = _configuration.LocalActor.Type;
            var actorName = _configuration.LocalActor.Name;
            var actorAddress = _configuration.LocalActor.Address;
            var actorPort = _configuration.LocalActor.Port;
            if (string.IsNullOrWhiteSpace(actorType))
                throw new InvalidProgramException(
                    string.Format("Local actor type cannot be empty."));
            if (string.IsNullOrWhiteSpace(actorName))
                throw new InvalidProgramException(
                    string.Format("Local actor name cannot be empty."));
            if (string.IsNullOrWhiteSpace(actorAddress))
                throw new InvalidProgramException(
                    string.Format("Local actor address cannot be empty."));
            if (string.IsNullOrWhiteSpace(actorPort))
                throw new InvalidProgramException(
                    string.Format("Local actor port cannot be empty."));

            var actor = new ActorIdentity(actorType, actorName);
            actor.Address = actorAddress;
            actor.Port = actorPort;

            return actor;
        }

        public static LocalXmlFileActorConfiguration Load(string localXmlFilePath)
        {
            var configuration = new LocalXmlFileActorConfiguration(localXmlFilePath);
            configuration.Build();
            return configuration;
        }
    }
}
