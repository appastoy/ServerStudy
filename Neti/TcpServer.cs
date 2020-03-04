using System;
using System.Net;
using System.Net.Sockets;

namespace Neti
{
    public class TcpServer : IDisposable
    {
        Action started;
        Action stopped;
        Action<TcpClient> sessionEntered;
        SocketAsyncEventArgs acceptAsyncEventArgs;
        SessionIdGenerator sessionIdGenerator = new SessionIdGenerator();

        public SessionCollection Sessions { get; private set; } = new SessionCollection();

        public IPEndPoint LocalEndPoint { get; private set; }
        public Socket Socket { get; private set; }
        public bool IsActive => Socket != null;

        public event Action<TcpClient> SessionEntered
        {
            add => sessionEntered += value;
            remove => sessionEntered -= value;
        }

        public event Action Started
        {
            add => started += value;
            remove => started -= value;
        }

        public event Action Stopped
        {
            add => stopped += value;
            remove => stopped -= value;
        }

        public void Start() => Start(new IPEndPoint(IPAddress.Any, 0));

        public void Start(int port) => Start(new IPEndPoint(IPAddress.Any, port));

        public void Start(string ip, int port)
        {
            if (IPAddress.TryParse(ip, out var ipAddress) == false)
            {
                throw new ArgumentException("Invalid ip");
            }

            Start(new IPEndPoint(ipAddress, port));
        }

        public void Start(IPAddress ip, int port)
        {
            if (ip is null)
            {
                throw new ArgumentNullException(nameof(ip));
            }

            Start(new IPEndPoint(ip, port));
        }

        public virtual void Start(IPEndPoint localEndPoint)
        {
            if (IsActive)
            {
                throw new InvalidOperationException("Already started.");
            }

            if (localEndPoint is null)
            {
                throw new ArgumentNullException(nameof(localEndPoint));
            }

            Validator.ValidatePort(localEndPoint.Port);

            try
            {
                LocalEndPoint = localEndPoint;

                EnsureAcceptEventArgs();
                EnsureSocket();
                Socket.Bind(localEndPoint);
                Socket.Listen((int)SocketOptionName.MaxConnections);

                AcceptAsync();

                OnStarted();
                started?.Invoke();
            }
            catch (Exception)
            {
                Socket?.Close();
                Socket = null;
                throw;
            }
        }

        protected virtual void OnStarted() 
        {

        }

        public virtual void Stop()
        {
            if (IsActive)
            {
                Sessions.Clear();
                Socket.Close();
                Socket = null;
                stopped?.Invoke();
                OnStopped();
            }
        }

        protected virtual void OnStopped()
        {

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void EnsureAcceptEventArgs()
        {
            if (acceptAsyncEventArgs == null)
            {
                acceptAsyncEventArgs = new SocketAsyncEventArgs();
                acceptAsyncEventArgs.Completed += OnAccept;
            }
        }

        void EnsureSocket()
        {
            if (Socket == null)
            {
                Socket = new Socket(LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    ExclusiveAddressUse = false
                };
                Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
        }

        void AcceptAsync()
        {
            if (Socket.AcceptAsync(acceptAsyncEventArgs) == false)
            {
                OnAccept(this, acceptAsyncEventArgs);
            }
        }

        void OnAccept(object _, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                var socket = e.AcceptSocket;
                e.AcceptSocket = null;

                var newSessionId = sessionIdGenerator.Generate();
                var newSession = CreateSession(socket, newSessionId);
                Sessions.Add(newSession);
                sessionEntered?.Invoke(newSession);

                AcceptAsync();
            }
            else
            {
                Stop();
            }
        }

        protected virtual TcpSession CreateSession(Socket socket, int id)
        {
            return new TcpSession(this, socket, id);
        }

        protected virtual void Dispose(bool _)
        {
            sessionEntered = null;
            started = null;

            Stop();

            acceptAsyncEventArgs?.Dispose();
            acceptAsyncEventArgs = null;
            stopped = null;
            Sessions?.Clear();
            Sessions = null;
            sessionIdGenerator = null;
        }

        ~TcpServer()
        {
            Dispose(false);
        }
    }
}
