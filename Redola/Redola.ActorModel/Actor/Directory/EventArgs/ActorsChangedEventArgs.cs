using System;
using System.Collections.Generic;
using System.Linq;

namespace Redola.ActorModel
{
    public class ActorsChangedEventArgs : EventArgs
    {
        public ActorsChangedEventArgs(IEnumerable<ActorIdentity> actors)
        {
            if (actors == null)
                throw new ArgumentNullException("actors");

            this.Actors = actors;
        }

        public IEnumerable<ActorIdentity> Actors { get; private set; }

        public override string ToString()
        {
            return string.Format("ActorCount[{0}]", Actors.Count());
        }
    }
}
