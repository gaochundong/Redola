using System;
using System.Xml.Serialization;
using ProtoBuf;

namespace Redola.Rpc
{
    [Serializable]
    [XmlType(TypeName = "Message")]
    [ProtoContract(SkipConstructor = false, UseProtoMembersOnly = true)]
    public sealed class ActorMessageEnvelope<T>
    {
        private static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public ActorMessageEnvelope()
        {
            this.MessageID = Guid.NewGuid().ToString();
            this.MessageTime = DateTime.UtcNow;
            this.MessageType = typeof(T).Name;
        }

        [ProtoMember(10)]
        public string MessageID { get; set; }
        [ProtoMember(20, Name = @"MessageTime")]
        public long SerializedMessageTime { get; set; }
        [ProtoIgnore]
        public DateTime MessageTime
        {
            get
            {
                return _unixEpoch.AddTicks(this.SerializedMessageTime);
            }
            set
            {
                this.SerializedMessageTime = value.Subtract(_unixEpoch).Ticks;
            }
        }

        [ProtoMember(30)]
        public string CorrelationID { get; set; }
        [ProtoMember(40, Name = @"CorrelationTime")]
        public long SerializedCorrelationTime { get; set; }
        [ProtoIgnore]
        public DateTime CorrelationTime
        {
            get
            {
                return _unixEpoch.AddTicks(this.SerializedCorrelationTime);
            }
            set
            {
                this.SerializedCorrelationTime = value.Subtract(_unixEpoch).Ticks;
            }
        }

        [ProtoMember(80)]
        public string MessageType { get; set; }

        [XmlIgnore]
        public T Message { get; set; }

        public ActorMessageEnvelope ConvertToNonGeneric()
        {
            var envelope = new ActorMessageEnvelope();
            envelope.CopyFrom(this);
            return envelope;
        }

        public static ActorMessageEnvelope<T> NewFrom(ActorMessageEnvelope source)
        {
            var envelope = new ActorMessageEnvelope<T>();
            envelope.CopyFrom(source);
            return envelope;
        }

        public void CopyFrom(ActorMessageEnvelope source)
        {
            this.MessageID = source.MessageID;
            this.MessageTime = source.MessageTime;

            this.CorrelationID = source.CorrelationID;
            this.CorrelationTime = source.CorrelationTime;

            this.MessageType = source.MessageType;
        }

        public override string ToString()
        {
            return string.Format("MessageType[{0}], MessageID[{1}], MessageTime[{2}], CorrelationID[{3}], CorrelationTime[{4}]",
                MessageType,
                MessageID, MessageTime.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"),
                CorrelationID, CorrelationTime.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"));
        }
    }
}
