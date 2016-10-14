using System;

namespace Redola.ActorModel.Framing
{
    [Serializable]
    public class ActorFrameException : Exception
    {
        public ActorFrameException(string message)
            : base(message)
        {
        }

        public ActorFrameException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
