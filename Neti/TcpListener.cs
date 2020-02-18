using System;
using System.Net;
using System.Net.Sockets;

namespace Neti
{
    public class TcpListener : IDisposable
    {
        Action<TcpClient> _clientEntered;
        SocketAsyncEventArgs _acceptAsyncEventArgs;

        public IPEndPoint LocalEndPoint { get; }
        public Socket Socket { get; private set; }
        public bool IsActive => Socket != null;

        public event Action<TcpClient> NewClientEntered
        {
            add => _clientEntered += value;
            remove => _clientEntered -= value;
        }

        public TcpListener() : this(IPAddress.Any, 0)
        {
            
        }

        public TcpListener(int port) : this(IPAddress.Any, port)
        {
            
        }

        public TcpListener(string ip, int port)
        {
            Validator.ValidatePort(port);
            if (IPAddress.TryParse(ip, out var ipAddress) == false)
            {
                throw new ArgumentException("Invalid ip");
            }

            LocalEndPoint = new IPEndPoint(ipAddress, port);
        }

        public TcpListener(IPAddress ip, int port)
        {
            Validator.ValidatePort(port);
            LocalEndPoint = new IPEndPoint(ip, port);
        }

        public void Start()
        {
            if (IsActive)
            {
                throw new InvalidOperationException("Already started.");
            }

            try
            {
                EnsureAcceptEventArgs();
                EnsureSocket();
                Socket.Bind(LocalEndPoint);
                Socket.Listen((int)SocketOptionName.MaxConnections);

                AcceptAsync();
            }
            catch(Exception)
            {
                Socket?.Close();
                Socket = null;
                throw;
            }
        }

        public void Stop()
        {
            if (IsActive)
            {
                Socket.Close();
                Socket = null;
            }
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
                var newClient = new TcpClient(e.AcceptSocket);
                e.AcceptSocket = null;
                _clientEntered?.Invoke(newClient);

                AcceptAsync();
            }
            else
            {
                Stop();
            }
        }

        protected virtual void Dispose(bool _)
        {
            _clientEntered = null;

            Stop();

            _acceptAsyncEventArgs?.Dispose();
            _acceptAsyncEventArgs = null;
        }

        ~TcpListener()
        {
            Dispose(false);
        }
    }
}
