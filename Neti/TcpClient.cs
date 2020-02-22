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
		bool _disconnectRequired;

		public Socket Socket { get; private set; }

		public IPEndPoint RemoteEndPoint { get; private set; }
		public IPAddress Address => RemoteEndPoint.Address;
		public int Port => RemoteEndPoint.Port;
		public bool IsConnected { get; private set; }
		public bool IsDisposed => Socket == null;

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
			EnsureSocket();
			RemoteEndPoint = remoteEndPoint;
			
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
			EnsureSocket();
			RemoteEndPoint = remoteEndPoint;

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

		public void SendAsync(byte[] bytes)
		{
			SendAsync(bytes, 0, bytes != null ? bytes.Length : 0);
		}

		public void SendAsync(byte[] bytes, int offset, int count)
		{
			Validator.ValidateBytes(bytes, offset, count);

			var eventArgs = OnCreateEventArgs(OnSend, bytes, offset, count);
			
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

			ReceiveAsync();
		}

		void ReceiveAsync()
		{
			if (Socket != null &&
				Socket.Connected &&
				Socket.ReceiveAsync(_recvAsyncEventArgs) == false)
			{
				OnReceive(this, _recvAsyncEventArgs);
			}
		}

		void OnReceive(object _, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				if (e.BytesTransferred > 0)
				{
					_bytesReceived?.Invoke(new ArraySegment<byte>(e.Buffer, e.Offset, e.BytesTransferred));

					var streamBuffer = (StreamBuffer)e.UserToken;
					streamBuffer.ExternalWrite(e.BytesTransferred);
					
					OnBytesReceived(streamBuffer);

					_recvAsyncEventArgs.SetBuffer(streamBuffer.WritePosition, streamBuffer.WritableSize);

					ReceiveAsync();
				}
				else if (_disconnectRequired)
				{
					_disconnectRequired = false;
				}
				else
				{
					Close();
				}
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

		protected virtual SocketAsyncEventArgs OnCreateEventArgs(EventHandler<SocketAsyncEventArgs> handler, byte[] bytes, int offset, int count)
		{
			var eventArgs = new SocketAsyncEventArgs();
			eventArgs.Completed += handler;
			eventArgs.SetBuffer(bytes, offset, count);

			return eventArgs;
		}

		void OnSend(object _, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				_bytesSent?.Invoke(new ArraySegment<byte>(e.Buffer, e.Offset, e.BytesTransferred));
			}
			OnBytesSent(e);
		}

		protected virtual void OnBytesSent(SocketAsyncEventArgs e)
		{
			e?.Dispose();
		}

		void Disconnect(bool reuseSocket)
		{
			if (IsConnected)
			{
				_disconnectRequired = true;
				Socket.Shutdown(SocketShutdown.Both);
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

				_disconnectRequired = true;
				Socket.Shutdown(SocketShutdown.Both);
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
				if (IsConnected)
				{
					IsConnected = false;
					OnDisconnected();
					_disconnected?.Invoke();
				}
				Socket?.Close();
				Socket = null;
				_recvAsyncEventArgs?.Dispose();
				_recvAsyncEventArgs = null;
				_connectAsyncEventArgs?.Dispose();
				_connectAsyncEventArgs = null;
				_disconnectAsyncEventArgs?.Dispose();
				_disconnectAsyncEventArgs = null;
				_disconnectRequired = false;
				_connected = null;
				RemoteEndPoint = null;
			}
		}

		~TcpClient()
		{
			Dispose(false);
		}
	}
}
