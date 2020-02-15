using System;
using System.Net;
using System.Net.Sockets;

namespace Neti
{
	public class TcpClient : IDisposable
	{
		IPEndPoint _remoteEndPoint;
        Action<bool> _onConnectionChanged;
        SocketAsyncEventArgs _connectAsyncEventArgs;
		SocketAsyncEventArgs _recvAsyncEventArgs;

		public Socket Socket { get; private set; }
		public IPAddress Address => _remoteEndPoint.Address;
		public int Port => _remoteEndPoint.Port;
        public bool IsConnected => Socket != null && Socket.Connected;

		public event Action<bool> ConnectionChanged
		{
			add => _onConnectionChanged += value;
			remove => _onConnectionChanged -= value;
		}

		public TcpClient(string ip, int port)
		{
			if (ip is null)
			{
				throw new ArgumentNullException(nameof(ip));
			}

			if (IPAddress.TryParse(ip, out var ipAddress) == false)
			{
				throw new ArgumentException("Invalid ip.");
			}

			PortUtility.ValidatePort(port);
			_remoteEndPoint = new IPEndPoint(ipAddress, port);
			EnsureSocket();
		}

		public TcpClient(IPAddress ip, int port)
		{
			if (ip is null)
			{
				throw new ArgumentNullException(nameof(ip));
			}

			PortUtility.ValidatePort(port);
			_remoteEndPoint = new IPEndPoint(ip, port);
			EnsureSocket();
		}

		public TcpClient(IPEndPoint remoteEndPoint)
		{
			if (remoteEndPoint is null)
			{
				throw new ArgumentNullException(nameof(remoteEndPoint));
			}

			PortUtility.ValidatePort(remoteEndPoint.Port);
			_remoteEndPoint = remoteEndPoint;
			EnsureSocket();
		}

		void EnsureSocket()
		{
			if (Socket == null)
			{
				Socket = new Socket(_remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
				{
					ExclusiveAddressUse = false,
					NoDelay = true
				};
				Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			}
		}

		public TcpClient(Socket socket)
		{
			if (socket == null)
			{
				throw new ArgumentNullException(nameof(socket));
			}

			if (socket.Connected == false)
			{
				throw new ArgumentException("socket is not connected.", nameof(socket));
			}

			Socket = socket;
			_remoteEndPoint = Socket.RemoteEndPoint as IPEndPoint;
		}

		public void Connect()
		{
			if (IsConnected)
			{
				throw new InvalidOperationException("Already connected.");
			}

			Socket.Connect(_remoteEndPoint);
			_onConnectionChanged?.Invoke(true);
		}

		public void ConnectAsync()
		{
			if (IsConnected)
			{
				throw new InvalidOperationException("Already connected.");
			}

			EnsureConnectAsyncEventArgs();
			if (Socket.ConnectAsync(_connectAsyncEventArgs) == false)
			{
				OnConnect(this, _connectAsyncEventArgs);
			}
		}

		void EnsureConnectAsyncEventArgs()
		{
			if (_connectAsyncEventArgs == null)
			{
				_connectAsyncEventArgs = new SocketAsyncEventArgs
				{
					RemoteEndPoint = _remoteEndPoint
				};
				_connectAsyncEventArgs.Completed += OnConnect;
			}
		}

		void OnConnect(object _, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				_onConnectionChanged?.Invoke(true);
				BeginRecevie();
			}
			else
			{
				Disconnect();
			}
		}

		void BeginRecevie()
		{
			if (_recvAsyncEventArgs == null)
			{
				_recvAsyncEventArgs = new SocketAsyncEventArgs();
				_recvAsyncEventArgs.Completed += OnReceive;
			}

			// TODO: Set Buffer.

			ReceiveAsync();
		}

		void ReceiveAsync()
		{

		}


		void OnReceive(object _, SocketAsyncEventArgs e)
		{
			
		}

		public void Disconnect()
		{
			if (IsConnected)
			{
				Socket.Shutdown(SocketShutdown.Both);
				Socket.Disconnect(true);
				_onConnectionChanged?.Invoke(false);
			}
		}

        public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Socket?.Close();
			Socket = null;
			_connectAsyncEventArgs?.Dispose();
			_connectAsyncEventArgs = null;
			_onConnectionChanged?.Invoke(false);
			_onConnectionChanged = null;
		}

		~TcpClient()
		{
			Dispose(false);
		}
	}
}
