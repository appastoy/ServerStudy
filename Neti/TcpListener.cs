using System;
using System.Net;
using System.Net.Sockets;

namespace Neti
{
    public class TcpListener : IDisposable
    {
        Action _started;
        Action _stopped;
        Action<Socket> _clientEntered;
        SocketAsyncEventArgs _acceptAsyncEventArgs;

        public IPEndPoint LocalEndPoint { get; private set; }
        public Socket Socket { get; private set; }
        public bool IsActive => Socket != null;

        public event Action<Socket> NewClientEntered
        {
            add => _clientEntered += value;
            remove => _clientEntered -= value;
        }

        public event Action Started
        {
            add => _started += value;
            remove => _started -= value;
        }

        public event Action Stopped
        {
            add => _stopped += value;
            remove => _stopped -= value;
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
                _started?.Invoke();
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
                Socket.Close();
                Socket = null;
                _stopped?.Invoke();
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
            if (_acceptAsyncEventArgs == null)
            {
                _acceptAsyncEventArgs = new SocketAsyncEventArgs();
                _acceptAsyncEventArgs.Completed += OnAccept;
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
            if (Socket.AcceptAsync(_acceptAsyncEventArgs) == false)
            {
                OnAccept(this, _acceptAsyncEventArgs);
            }
        }

        void OnAccept(object _, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                var newClientSocket = e.AcceptSocket;
                e.AcceptSocket = null;
                OnClientEntered(newClientSocket);
                _clientEntered?.Invoke(newClientSocket);

                AcceptAsync();
            }
            else
            {
                Stop();
            }
        }

        protected virtual void OnClientEntered(Socket newClientSocket)
        {

        }

        protected virtual void Dispose(bool _)
        {
            _clientEntered = null;
            _started = null;

            Stop();

            _acceptAsyncEventArgs?.Dispose();
            _acceptAsyncEventArgs = null;
            _stopped = null;
        }

        ~TcpListener()
        {
            Dispose(false);
        }
    }
}
