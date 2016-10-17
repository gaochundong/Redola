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

        [ProtoMember(80)]
        public string MessageType { get; set; }
        [ProtoMember(90)]
        public byte[] MessageData { get; set; }

        public override string ToString()
        {
            return string.Format("MessageType[{0}], MessageID[{1}], MessageTime[{2}]",
                MessageType,
                MessageID, MessageTime.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"));
        }
    }
}
