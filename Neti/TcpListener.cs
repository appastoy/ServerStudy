using System;
using System.Net;
using System.Net.Sockets;

namespace Neti
{
    public class TcpListener : IDisposable
    {
        Action<Socket> _onClientEntered;
        SocketAsyncEventArgs _acceptAsyncEventArgs;

        public IPEndPoint LocalEndPoint { get; }
        public Socket Socket { get; private set; }
        public bool IsActive => Socket != null;

        public event Action<Socket> OnClientEntered
        {
            add => _onClientEntered += value;
            remove => _onClientEntered -= value;
        }

        public TcpListener()
        {
            var availablePort = PortUtility.FindAvailableTcpPort();
            if (availablePort.HasValue == false)
            {
                throw new InvalidOperationException("There is no available port.");
            }

            LocalEndPoint = new IPEndPoint(IPAddress.Any, availablePort.Value);
        }

        public TcpListener(int port) : this(IPAddress.Any, port)
        {
            
        }

        public TcpListener(string ip, int port)
        {
            PortUtility.ValidatePort(port);
            if (IPAddress.TryParse(ip, out var ipAddress) == false)
            {
                throw new ArgumentException("Invalid ip");
            }

            LocalEndPoint = new IPEndPoint(ipAddress, port);
        }

        public TcpListener(IPAddress ip, int port)
        {
            PortUtility.ValidatePort(port);
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
                CreateSocket();
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

        void EnsureAcceptEventArgs()
        {
            if (_acceptAsyncEventArgs == null)
            {
                _acceptAsyncEventArgs = new SocketAsyncEventArgs();
                _acceptAsyncEventArgs.Completed += OnAccept;
            }
        }

        void CreateSocket()
        {
            Socket = new Socket(LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                ExclusiveAddressUse = false
            };
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
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
                var acceptSocket = e.AcceptSocket;
                e.AcceptSocket = null;
                _onClientEntered?.Invoke(acceptSocket);

                AcceptAsync();
            }
            else
            {
                Stop();
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

        protected virtual void Dispose(bool _)
        {
            _onClientEntered = null;

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
