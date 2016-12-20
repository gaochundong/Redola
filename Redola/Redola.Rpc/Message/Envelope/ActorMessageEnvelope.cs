using System;
using ProtoBuf;

namespace Redola.Rpc
{
    [ProtoContract(SkipConstructor = false, UseProtoMembersOnly = true)]
    public class ActorMessageEnvelope
    {
        private static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public ActorMessageEnvelope()
        {
            this.MessageID = Guid.NewGuid().ToString();
            this.MessageTime = DateTime.UtcNow;
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

        [ProtoMember(50)]
        public ActorEndpoint SourceEndpoint { get; set; }
        [ProtoMember(60)]
        public ActorEndpoint TargetEndpoint { get; set; }

        [ProtoMember(80)]
        public string MessageType { get; set; }
        [ProtoMember(90)]
        public byte[] MessageData { get; set; }

        public ActorMessageEnvelope<T> ConvertTo<T>()
        {
            var envelope = new ActorMessageEnvelope<T>();
            envelope.CopyFrom(this);
            return envelope;
        }

        public static ActorMessageEnvelope NewFrom<T>(ActorMessageEnvelope<T> source)
        {
            var envelope = new ActorMessageEnvelope();
            envelope.CopyFrom(source);
            return envelope;
        }

        public void CopyFrom<T>(ActorMessageEnvelope<T> source)
        {
            this.MessageID = source.MessageID;
            this.MessageTime = source.MessageTime;

            this.CorrelationID = source.CorrelationID;
            this.CorrelationTime = source.CorrelationTime;

            this.SourceEndpoint = source.SourceEndpoint == null ? null : source.SourceEndpoint.Clone();
            this.TargetEndpoint = source.TargetEndpoint == null ? null : source.TargetEndpoint.Clone();

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
