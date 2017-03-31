using System;
using System.IO;
using Redola.ActorModel.Serialization;

namespace Redola.ActorModel
{
    public class LocalXmlFileActorConfiguration : ActorConfiguration
    {
        private string _localXmlFilePath = string.Empty;

        public LocalXmlFileActorConfiguration()
            : this(Environment.CurrentDirectory + @"\\ActorConfiguration.xml")
        {
        }

        public LocalXmlFileActorConfiguration(string localXmlFilePath)
        {
            if (string.IsNullOrEmpty(localXmlFilePath))
                throw new ArgumentNullException("localXmlFilePath");
            if (!File.Exists(localXmlFilePath))
                throw new FileNotFoundException("Cannot find the xml actor configuration file.", localXmlFilePath);
            _localXmlFilePath = localXmlFilePath;
        }

        public string LocalXmlFilePath
        {
            get { return _localXmlFilePath; }
        }

        protected override ActorIdentity BuildCenterActor()
        {
            if (!File.Exists(LocalXmlFilePath))
                throw new FileNotFoundException("Cannot find the xml actor configuration file.", LocalXmlFilePath);

            var fileContent = File.ReadAllText(LocalXmlFilePath);
            var xmlActorConfiguration = XmlConvert.DeserializeObject<XmlActorConfiguration>(fileContent);

            var actorType = xmlActorConfiguration.CenterActor.Type;
            var actorName = xmlActorConfiguration.CenterActor.Name;
            var actorAddress = xmlActorConfiguration.CenterActor.Address;
            var actorPort = xmlActorConfiguration.CenterActor.Port;
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
            if (!File.Exists(LocalXmlFilePath))
                throw new FileNotFoundException("Cannot find the xml actor configuration file.", LocalXmlFilePath);

            var fileContent = File.ReadAllText(LocalXmlFilePath);
            var xmlActorConfiguration = XmlConvert.DeserializeObject<XmlActorConfiguration>(fileContent);

            var actorType = xmlActorConfiguration.LocalActor.Type;
            var actorName = xmlActorConfiguration.LocalActor.Name;
            var actorAddress = xmlActorConfiguration.LocalActor.Address;
            var actorPort = xmlActorConfiguration.LocalActor.Port;
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

        public static LocalXmlFileActorConfiguration Load()
        {
            var configuration = new LocalXmlFileActorConfiguration();
            configuration.Build();
            return configuration;
        }
    }
}
