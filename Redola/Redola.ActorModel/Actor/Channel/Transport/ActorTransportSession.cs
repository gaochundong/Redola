using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Cowboy.Sockets;

namespace Redola.ActorModel
{
    internal class ActorTransportSession
    {
        private TcpSocketSession _session;

        public ActorTransportSession(TcpSocketSession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            _session = session;
        }

        public string SessionKey { get { return _session.SessionKey; } }
        public DateTime StartTime { get { return _session.StartTime; } }
        public bool Active { get { return _session.Active; } }
        public IPEndPoint RemoteEndPoint { get { return _session.RemoteEndPoint; } }
        public IPEndPoint LocalEndPoint { get { return _session.LocalEndPoint; } }
        public Socket Socket { get { return _session.Socket; } }
        public Stream Stream { get { return _session.Stream; } }
        public TcpSocketServer Server { get { return _session.Server; } }
        public TimeSpan ConnectTimeout { get { return _session.ConnectTimeout; } }

        public void Close()
        {
            _session.Close();
        }

        public void Send(byte[] data)
        {
            Send(data, 0, data.Length);
        }

        public void Send(byte[] data, int offset, int count)
        {
            _session.Send(data, offset, count);
        }

        public void BeginSend(byte[] data)
        {
            BeginSend(data, 0, data.Length);
        }

        public void BeginSend(byte[] data, int offset, int count)
        {
            _session.SendAsync(data, offset, count);
        }

        public IAsyncResult BeginSend(byte[] data, AsyncCallback callback, object state)
        {
            return BeginSend(data, 0, data.Length, callback, state);
        }

        public IAsyncResult BeginSend(byte[] data, int offset, int count, AsyncCallback callback, object state)
        {
            return _session.BeginSend(data, offset, count, callback, state);
        }

        public void EndSend(IAsyncResult asyncResult)
        {
            _session.EndSend(asyncResult);
        }
    }
}
