using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Redola.ActorModel
{
    public class BlockingRouteActor : RouteActor
    {
        private ILog _log = Logger.Get<BlockingRouteActor>();
        private ConcurrentDictionary<string, BlockingCallbackHolder> _callbacks
            = new ConcurrentDictionary<string, BlockingCallbackHolder>();

        public BlockingRouteActor(ActorConfiguration configuration)
            : base(configuration)
        {
        }

        public P SendMessage<R, P>(string remoteActorType, MessageEnvelope<R> request)
        {
            return SendMessage<R, P>(remoteActorType, request, TimeSpan.FromSeconds(30));
        }

        public P SendMessage<R, P>(string remoteActorType, MessageEnvelope<R> request, TimeSpan timeout)
        {
            P response = default(P);
            Action<P> callback = (r) => { response = r; };

            try
            {
                ManualResetEvent waiter = new ManualResetEvent(false);
                _callbacks.Add(request.MessageID, new BlockingCallbackHolder(request.MessageID, waiter, callback));

                this.SendAsync(remoteActorType, request.ToBytes(this.Encoder));

                if (!waiter.WaitOne(timeout))
                {
                    _log.ErrorFormat("Timeout when waiting message [{0}] after {1} seconds.",
                        request.MessageType, timeout.TotalSeconds);
                }
                waiter.Reset();
                waiter.Dispose();
                BlockingCallbackHolder throwAway = null;
                _callbacks.TryRemove(request.MessageID, out throwAway);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return response;
        }

        public void OnSyncMessage<P>(ActorDescription remoteActor, MessageEnvelope<P> response)
        {
            BlockingCallbackHolder callback = null;
            if (_callbacks.TryRemove(response.CorrelationID, out callback)
                && callback != null)
            {
                var action = callback.Action as Action<P>;
                if (action != null)
                {
                    action.Invoke(response.Message);
                }

                try
                {
                    callback.Waiter.Set();
                }
                catch (ObjectDisposedException) { }
            }
        }
    }
}
