using System;
using System.Collections.Concurrent;
using System.Threading;
using Logrila.Logging;
using Redola.ActorModel;
using Redola.ActorModel.Extensions;

namespace Redola.Rpc
{
    public class BlockingRouteActor : RouteActor
    {
        private ILog _log = Logger.Get<BlockingRouteActor>();
        private ConcurrentDictionary<string, BlockingCallbackHolder> _callbacks
            = new ConcurrentDictionary<string, BlockingCallbackHolder>();

        public BlockingRouteActor(
            ActorConfiguration configuration,
            IActorMessageEncoder encoder,
            IActorMessageDecoder decoder)
            : base(configuration, encoder, decoder)
        {
        }

        #region Blocking

        public ActorMessageEnvelope<P> SendMessage<R, P>(ActorIdentity remoteActor, ActorMessageEnvelope<R> request)
        {
            return SendMessage<R, P>(remoteActor, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> SendMessage<R, P>(ActorIdentity remoteActor, ActorMessageEnvelope<R> request, TimeSpan timeout)
        {
            return SendMessage<R, P>(remoteActor.Type, remoteActor.Name, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> SendMessage<R, P>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<R> request)
        {
            return SendMessage<R, P>(remoteActorType, remoteActorName, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> SendMessage<R, P>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<R> request, TimeSpan timeout)
        {
            ActorMessageEnvelope<P> response = default(ActorMessageEnvelope<P>);
            Action<ActorMessageEnvelope<P>> callback = (r) => { response = r; };

            try
            {
                ManualResetEvent waiter = new ManualResetEvent(false);
                _callbacks.Add(request.MessageID, new BlockingCallbackHolder(request.MessageID, waiter, callback));

                this.BeginSend(remoteActorType, remoteActorName, request.ToBytes(this.Encoder));

                bool responseTimeout = false;
                if (!waiter.WaitOne(timeout))
                {
                    responseTimeout = true;
                }
                waiter.Reset();
                waiter.Dispose();
                BlockingCallbackHolder throwAway = null;
                _callbacks.TryRemove(request.MessageID, out throwAway);

                if (responseTimeout)
                {
                    throw new TimeoutException(string.Format(
                        "Timeout when waiting message [{0}] after [{1}] seconds.",
                        request.MessageType, timeout.TotalSeconds));
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }

            return response;
        }

        public ActorMessageEnvelope<P> SendMessage<R, P>(string remoteActorType, ActorMessageEnvelope<R> request)
        {
            return SendMessage<R, P>(remoteActorType, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> SendMessage<R, P>(string remoteActorType, ActorMessageEnvelope<R> request, TimeSpan timeout)
        {
            ActorMessageEnvelope<P> response = default(ActorMessageEnvelope<P>);
            Action<ActorMessageEnvelope<P>> callback = (r) => { response = r; };

            try
            {
                ManualResetEvent waiter = new ManualResetEvent(false);
                _callbacks.Add(request.MessageID, new BlockingCallbackHolder(request.MessageID, waiter, callback));

                this.BeginSend(remoteActorType, request.ToBytes(this.Encoder));

                bool responseTimeout = false;
                if (!waiter.WaitOne(timeout))
                {
                    responseTimeout = true;
                }
                waiter.Reset();
                waiter.Dispose();
                BlockingCallbackHolder throwAway = null;
                _callbacks.TryRemove(request.MessageID, out throwAway);

                if (responseTimeout)
                {
                    throw new TimeoutException(string.Format(
                        "Timeout when waiting message [{0}] after [{1}] seconds.",
                        request.MessageType, timeout.TotalSeconds));
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }

            return response;
        }

        #endregion

        public void OnSyncMessage<P>(ActorSender sender, ActorMessageEnvelope<P> response)
        {
            if (response == null
                || string.IsNullOrWhiteSpace(response.CorrelationID))
                throw new InvalidOperationException("Invalid or empty message CorrelationID.");

            BlockingCallbackHolder callback = null;
            if (_callbacks.TryRemove(response.CorrelationID, out callback)
                && callback != null)
            {
                var action = callback.Action as Action<ActorMessageEnvelope<P>>;
                if (action != null)
                {
                    action.Invoke(response);
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
