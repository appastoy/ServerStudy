using System;
using System.Net;
using System.Net.Sockets;
using Neti.Buffer;
using Neti.Pool;

namespace Neti
{
	public class TcpClient : IDisposable
	{
		public static TcpClient CreateFromSocket(Socket socket)
		{
			var newClient = new TcpClient();
			newClient.OnAccepted(socket);

			return newClient;
		}

		public static T CreateFromSocket<T>(Socket socket) where T : TcpClient, new()
		{
			var newClient = new T();
			newClient.OnAccepted(socket);

			return newClient;
		}

		Action _connected;
		Action _disconnected;
		Action<ArraySegment<byte>> _bytesReceived;
		Action<ArraySegment<byte>> _bytesSent;
		SocketAsyncEventArgs _connectAsyncEventArgs;
		SocketAsyncEventArgs _disconnectAsyncEventArgs;
		SocketAsyncEventArgs _recvAsyncEventArgs;
		GenericPool<SocketAsyncEventArgs> _sendAsyncEventArgsPool;
		bool _disconnectRequested;

		public Socket Socket { get; private set; }

		public IPEndPoint RemoteEndPoint { get; private set; }
		public IPAddress Address => RemoteEndPoint.Address;
		public int Port => RemoteEndPoint.Port;
		public bool IsConnected { get; private set; }
		public bool IsDisposed { get; private set; }

		public event Action Connected
		{
			add => _connected += value;
			remove => _connected -= value;
		}

		public event Action Disconnected
		{
			add => _disconnected += value;
			remove => _disconnected -= value;
		}

		public event Action<ArraySegment<byte>> BytesReceived
		{
			add => _bytesReceived += value;
			remove => _bytesReceived -= value;
		}

		public event Action<ArraySegment<byte>> BytesSent
		{
			add => _bytesSent += value;
			remove => _bytesSent -= value;
		}

		public void SetSendEventArgsPool(GenericPool<SocketAsyncEventArgs> pool)
		{
			_sendAsyncEventArgsPool = pool ?? throw new ArgumentNullException(nameof(pool));
		}

		public void Connect(string ip, int port)
		{
			var ipAddress = Validator.ValidateAndParseIp(ip);

			Connect(new IPEndPoint(ipAddress, port));
		}

		public void Connect(IPAddress ip, int port)
		{
			if (ip is null)
			{
				throw new ArgumentNullException(nameof(ip));
			}

			Connect(new IPEndPoint(ip, port));
		}

		public virtual void Connect(IPEndPoint remoteEndPoint)
		{
			CheckDisposed();
			CheckConnected();

			if (remoteEndPoint is null)
			{
				throw new ArgumentNullException(nameof(remoteEndPoint));
			}

			Validator.ValidatePort(remoteEndPoint.Port);
			RemoteEndPoint = remoteEndPoint;
			EnsureSocket();

			Socket.Connect(RemoteEndPoint);
			IsConnected = true;
			_connected?.Invoke();
			BeginRecevie();
		}

		public void ConnectAsync(string ip, int port)
		{
			var ipAddress = Validator.ValidateAndParseIp(ip);

			ConnectAsync(new IPEndPoint(ipAddress, port));
		}

		public void ConnectAsync(IPAddress ip, int port)
		{
			if (ip is null)
			{
				throw new ArgumentNullException(nameof(ip));
			}

			ConnectAsync(new IPEndPoint(ip, port));
		}

		public virtual void ConnectAsync(IPEndPoint remoteEndPoint)
		{
			CheckDisposed();
			CheckConnected();

			if (remoteEndPoint is null)
			{
				throw new ArgumentNullException(nameof(remoteEndPoint));
			}

			Validator.ValidatePort(remoteEndPoint.Port);
			RemoteEndPoint = remoteEndPoint;
			EnsureSocket();

			if (_connectAsyncEventArgs == null)
			{
				_connectAsyncEventArgs = new SocketAsyncEventArgs { RemoteEndPoint = RemoteEndPoint };
				_connectAsyncEventArgs.Completed += OnConnect;
			}

			if (Socket.ConnectAsync(_connectAsyncEventArgs) == false)
			{
				OnConnect(this, _connectAsyncEventArgs);
			}
		}

		public void Send(byte[] bytes)
		{
			Send(bytes, 0, bytes is null ? 0 : bytes.Length);
		}

		public void Send(byte[] bytes, int offset, int count)
		{
			Validator.ValidateBytes(bytes, offset, count);

			var bytesCount = Socket.Send(bytes, offset, count, SocketFlags.None);
			_bytesSent?.Invoke(new ArraySegment<byte>(bytes, offset, bytesCount));
		}

		public void SendAsync(byte[] bytes, object userToken = null)
		{
			SendAsync(bytes, 0, bytes != null ? bytes.Length : 0, userToken);
		}

		public void SendAsync(byte[] bytes, int offset, int count, object userToken = null)
		{
			Validator.ValidateBytes(bytes, offset, count);

			if (_sendAsyncEventArgsPool == null)
			{
				_sendAsyncEventArgsPool = new GenericPool<SocketAsyncEventArgs>();
			}

			var (eventArgs, isCreated) = _sendAsyncEventArgsPool.AllocEx();
			if (isCreated)
			{
				eventArgs.Completed += OnSend;
			}
			eventArgs.SetBuffer(bytes, offset, count);
			eventArgs.UserToken = userToken;

			if (Socket.SendAsync(eventArgs) == false)
			{
				OnSend(this, eventArgs);
			}
		}

		public void Disconnect()
		{
			Disconnect(true);
		}

		public void DisconnectAsync()
		{
			DisconnectAsync(true);
		}

		public void DisconnectAndClose()
		{
			Disconnect(false);
		}

		public void DisconnectAndCloseAsync()
		{
			DisconnectAsync(false);
		}

		public void Close()
		{
			Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void OnAccepted(Socket socket)
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
			RemoteEndPoint = Socket.RemoteEndPoint as IPEndPoint;
			IsConnected = true;
			BeginRecevie();
		}

		void EnsureSocket()
		{
			if (Socket == null)
			{
				Socket = new Socket(RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
				{
					ExclusiveAddressUse = false,
					NoDelay = true
				};
				Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			}
		}

		void CheckDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(nameof(TcpClient));
			}
		}

		void CheckConnected()
		{
			if (IsConnected)
			{
				throw new InvalidOperationException("Already connected.");
			}
		}

		void OnConnect(object _, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				IsConnected = true;
				OnConnected();
				_connected?.Invoke();
				BeginRecevie();
			}
		}

		protected virtual void OnConnected()
		{

		}

		void BeginRecevie()
		{
			if (_recvAsyncEventArgs == null)
			{
				_recvAsyncEventArgs = new SocketAsyncEventArgs();
				_recvAsyncEventArgs.Completed += OnReceive;

				// TODO: Optimize buffer.
				var buffer = new StreamBuffer();
				_recvAsyncEventArgs.UserToken = buffer;
				_recvAsyncEventArgs.SetBuffer(buffer.Buffer, 0, buffer.WritableSize);
			}

			ReceiveAsync(_recvAsyncEventArgs);
		}

		void ReceiveAsync(SocketAsyncEventArgs e)
		{
			if (Socket != null &&
				Socket.Connected &&
				Socket.ReceiveAsync(e) == false)
			{
				OnReceive(this, e);
			}
		}

		void OnReceive(object _, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success &&
				e.BytesTransferred > 0)
			{
				_bytesReceived?.Invoke(new ArraySegment<byte>(e.Buffer, e.Offset, e.BytesTransferred));

				var streamBuffer = (StreamBuffer)e.UserToken;
				streamBuffer.ExternalWrite(e.BytesTransferred);

				OnBytesReceived(streamBuffer);

				e.SetBuffer(streamBuffer.WritePosition, streamBuffer.WritableSize);

				ReceiveAsync(e);
			}
			else if (_disconnectRequested)
			{
				_disconnectRequested = false;
			}
			else
			{
				Close();
			}
		}

		protected virtual void OnBytesReceived(IStreamBufferReader reader)
		{
			reader.ExternalRead(reader.ReadableSize);
		}

		static void OnSend(object sender, SocketAsyncEventArgs e)
		{
			var client = sender as TcpClient ?? throw new ArgumentException("sender is not TcpClient.", nameof(sender));
			if (e.SocketError == SocketError.Success)
			{
				client._bytesSent?.Invoke(new ArraySegment<byte>(e.Buffer, e.Offset, e.BytesTransferred));
			}
			client.OnBytesSent(e);
			e.SetBuffer(null, 0, 0);
			client._sendAsyncEventArgsPool.Free(e);
		}

		protected virtual void OnBytesSent(SocketAsyncEventArgs e)
		{

		}

		void Disconnect(bool reuseSocket)
		{
			if (IsConnected)
			{
				_disconnectRequested = true;
				try
				{
					Socket.Shutdown(SocketShutdown.Both);
				}
				catch (Exception)
				{ 

				}
				Socket.Disconnect(reuseSocket);
				if (reuseSocket)
				{
					IsConnected = false;
					_disconnected?.Invoke();
				}
				else
				{
					Close();
				}
			}
		}

		void DisconnectAsync(bool reuseSocket)
		{
			if (IsConnected)
			{
				if (_disconnectAsyncEventArgs == null)
				{
					_disconnectAsyncEventArgs = new SocketAsyncEventArgs();
					_disconnectAsyncEventArgs.Completed += OnDisconnect;
				}
				_disconnectAsyncEventArgs.DisconnectReuseSocket = reuseSocket;

				_disconnectRequested = true;

				try
				{
					Socket.Shutdown(SocketShutdown.Both);
				}
				catch (Exception)
				{

				}
				Socket.DisconnectAsync(_disconnectAsyncEventArgs);
			}
		}

		void OnDisconnect(object _, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				if (e.DisconnectReuseSocket)
				{
					IsConnected = false;
					OnDisconnected();
					_disconnected?.Invoke();
				}
				else
				{
					Close();
				}
			}
		}

		protected virtual void OnDisconnected()
		{

		}

		protected virtual void Dispose(bool disposing)
		{
			if (IsDisposed == false)
			{
				IsDisposed = true;
				
				_recvAsyncEventArgs?.Dispose();
				_recvAsyncEventArgs = null;
				_connectAsyncEventArgs?.Dispose();
				_connectAsyncEventArgs = null;
				_disconnectAsyncEventArgs?.Dispose();
				_disconnectAsyncEventArgs = null;
				_disconnectRequested = false;
				_connected = null;

				Socket?.Close();
				Socket = null;
				RemoteEndPoint = null;

				if (IsConnected)
				{
					IsConnected = false;
					OnDisconnected();
					_disconnected?.Invoke();
				}

				_disconnected = null;
			}
		}

		~TcpClient()
		{
			Dispose(false);
		}
	}
}
