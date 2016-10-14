using System;

namespace Redola.ActorModel
{
    [Serializable]
    public class ActorNotFoundException : Exception
    {
        public ActorNotFoundException(string message)
            : base(message)
        {
        }

        public ActorNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
