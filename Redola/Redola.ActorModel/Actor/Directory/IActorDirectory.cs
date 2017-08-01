using System;
using System.Collections.Generic;
using System.Net;

namespace Redola.ActorModel
{
    public interface IActorDirectory
    {
        void Open();
        void Close();
        bool Active { get; }

        void Register(ActorIdentity localActor);
        void Deregister(ActorIdentity localActor);

        IPEndPoint LookupRemoteActorEndPoint(string actorType, string actorName);
        IEnumerable<IPEndPoint> LookupRemoteActorEndPoints(string actorType);
        IEnumerable<ActorIdentity> LookupRemoteActors(string actorType);

        event EventHandler<ActorsChangedEventArgs> ActorsChanged;
    }
}
