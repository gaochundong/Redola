using System;
using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public interface IServiceDirectory
    {
        void RegisterService(ActorIdentity actor, Type serviceType);
        void RegisterService(ActorIdentity actor, Type serviceType, IEnumerable<string> tags);
        void DeregisterService(ActorIdentity actor, Type serviceType);
        void DeregisterService(string actorType, string actorName, Type serviceType);
        IEnumerable<ActorIdentity> GetActors(Type serviceType);
    }
}
