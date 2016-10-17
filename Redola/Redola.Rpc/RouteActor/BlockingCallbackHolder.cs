using System.Threading;

namespace Redola.Rpc
{
    public class BlockingCallbackHolder
    {
        public BlockingCallbackHolder()
        {
        }

        public BlockingCallbackHolder(string messageID, ManualResetEvent waiter, object action)
        {
            this.MessageID = messageID;
            this.Waiter = waiter;
            this.Action = action;
        }

        public string MessageID { get; set; }
        public ManualResetEvent Waiter { get; set; }
        public object Action { get; set; }
    }
}
