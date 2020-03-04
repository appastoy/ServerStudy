using System;
using System.Net;
using System.Net.Sockets;
using Neti.Buffer;
using Neti.Packets;
using Neti.Pool;

namespace Neti
{
	public class TcpClient : IDisposable
	{
		Action connected;
		Action disconnected;
		Action<PacketReader> packetReceived;
		Action<ArraySegment<byte>> bytesSent;

		StreamBuffer receiveBuffer;
		StreamBuffer sendBuffer;
		SocketAsyncEventArgs connectAsyncEventArgs;
		SocketAsyncEventArgs disconnectAsyncEventArgs;
		SocketAsyncEventArgs recvAsyncEventArgs;
		GenericPool<SocketAsyncEventArgs> sendAsyncEventArgsPool;
		bool disconnectRequested;

		public Socket Socket { get; protected set; }

		public IPEndPoint RemoteEndPoint { get; protected set; }
		public IPAddress RemoteAddress => RemoteEndPoint.Address;
		public int RemotePort => RemoteEndPoint.Port;
		public IPEndPoint LocalEndPoint => Socket?.LocalEndPoint as IPEndPoint;

		public bool IsConnected { get; protected set; }
		public bool IsDisposed { get; private set; }

		public event Action Connected
		{
			add => connected += value;
			remove => connected -= value;
		}

		public event Action Disconnected
		{
			add => disconnected += value;
			remove => disconnected -= value;
		}

		public event Action<PacketReader> PacketReceived
		{
			add => packetReceived += value;
			remove => packetReceived -= value;
		}

		public event Action<ArraySegment<byte>> BytesSent
		{
			add => bytesSent += value;
			remove => bytesSent -= value;
		}

		public TcpClient() : this(new StreamBuffer(), new StreamBuffer())
		{

		}

		public TcpClient(StreamBuffer receiveBuffer, StreamBuffer sendBuffer)
		{
			this.receiveBuffer = receiveBuffer ?? throw new ArgumentNullException(nameof(receiveBuffer));
			this.sendBuffer = sendBuffer ?? throw new ArgumentNullException(nameof(sendBuffer));
		}

		public void SetSendEventArgsPool(GenericPool<SocketAsyncEventArgs> pool)
		{
			sendAsyncEventArgsPool = pool ?? throw new ArgumentNullException(nameof(pool));
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
			connected?.Invoke();
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

			if (connectAsyncEventArgs == null)
			{
				connectAsyncEventArgs = new SocketAsyncEventArgs { RemoteEndPoint = RemoteEndPoint };
				connectAsyncEventArgs.Completed += OnConnect;
			}

			if (Socket.ConnectAsync(connectAsyncEventArgs) == false)
			{
				OnConnect(this, connectAsyncEventArgs);
			}
		}

		public int Send(byte[] bytes)
		{
			return Send(bytes, 0, bytes != null ? bytes.Length : 0);
		}

		public int Send(byte[] bytes, int offset, int count)
		{
			Validator.ValidateBytes(bytes, offset, count);

			CheckDisposed();

			using (var writer = CreatePacketWriter())
			{
				writer.Write(bytes, offset, count);
			}
			return FlushPackets();
		}

		public void SendAsync(byte[] bytes)
		{
			SendAsync(bytes, 0, bytes != null ? bytes.Length : 0);
		}

		public void SendAsync(byte[] bytes, int offset, int count)
		{
			Validator.ValidateBytes(bytes, offset, count);

			CheckDisposed();

			using (var writer = CreatePacketWriter())
			{
				writer.Write(bytes, offset, count);
			}
			FlushPacketsAsync();
		}

		public PacketWriter CreatePacketWriter()
		{
			return new PacketWriter(sendBuffer);
		}

		public int FlushPackets()
		{
			CheckDisposed();

			if (IsConnected &&
				sendBuffer.ProcessableSize > 0)
			{
				var bytesCount = Socket.Send(sendBuffer.Buffer, sendBuffer.ProcessingPosition, sendBuffer.ProcessableSize, SocketFlags.None);
				sendBuffer.ExternalProcess(sendBuffer.ProcessableSize);
				sendBuffer.ExternalRead(bytesCount);

				return bytesCount;
			}

			return 0;
		}

		public void FlushPacketsAsync()
		{
			CheckDisposed();

			if (IsConnected &&
				sendBuffer.ProcessableSize > 0)
			{
				if (sendAsyncEventArgsPool == null)
				{
					sendAsyncEventArgsPool = new GenericPool<SocketAsyncEventArgs>();
				}

				var (eventArgs, isCreated) = sendAsyncEventArgsPool.AllocEx();
				if (isCreated)
				{
					eventArgs.Completed += OnSend;
				}

				eventArgs.SetBuffer(sendBuffer.Buffer, sendBuffer.ProcessingPosition, sendBuffer.ProcessableSize);
				sendBuffer.ExternalProcess(sendBuffer.ProcessableSize);
				eventArgs.UserToken = sendBuffer;

				if (Socket.SendAsync(eventArgs) == false)
				{
					OnSend(this, eventArgs);
				}
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
				connected?.Invoke();
				BeginRecevie();
			}
		}

		protected virtual void OnConnected()
		{

		}

		protected void BeginRecevie()
		{
			if (recvAsyncEventArgs == null)
			{
				recvAsyncEventArgs = new SocketAsyncEventArgs();
				recvAsyncEventArgs.Completed += OnReceive;
				recvAsyncEventArgs.UserToken = receiveBuffer;
				recvAsyncEventArgs.SetBuffer(receiveBuffer.Buffer, 0, receiveBuffer.WritableSize);
			}

			ReceiveAsync(recvAsyncEventArgs);
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
				var streamBuffer = (StreamBuffer)e.UserToken;
				streamBuffer.ExternalWrite(e.BytesTransferred);

				while (streamBuffer.ReadableSize > 2)
				{
					var packetSize = streamBuffer.Peek<ushort>();
					var totalSize = packetSize + 2;
					if (streamBuffer.ReadableSize < totalSize)
					{
						break;
					}

					var packetReader = new PacketReader(streamBuffer, totalSize);
					packetReceived?.Invoke(packetReader);
					OnPacketReceived(packetReader);
				}

				e.SetBuffer(streamBuffer.WritePosition, streamBuffer.WritableSize);

				ReceiveAsync(e);
			}
			else if (disconnectRequested)
			{
				disconnectRequested = false;
			}
			else
			{
				Close();
			}
		}

		protected virtual void OnPacketReceived(PacketReader reader) 
		{
			reader.Use();
		}

		static void OnSend(object sender, SocketAsyncEventArgs e)
		{
			var client = sender as TcpClient ?? throw new InvalidOperationException("sender is not a TcpClient.");
			if (e.SocketError == SocketError.Success)
			{
				client.bytesSent?.Invoke(new ArraySegment<byte>(e.Buffer, e.Offset, e.BytesTransferred));
			}
			
			var buffer = (StreamBuffer)e.UserToken;
			buffer.ExternalRead(e.BytesTransferred);
			e.UserToken = null;
			e.SetBuffer(null, 0, 0);
			client.sendAsyncEventArgsPool.Free(e);
		}

		void Disconnect(bool reuseSocket)
		{
			if (IsConnected)
			{
				disconnectRequested = true;
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
					disconnected?.Invoke();
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
				if (disconnectAsyncEventArgs == null)
				{
					disconnectAsyncEventArgs = new SocketAsyncEventArgs();
					disconnectAsyncEventArgs.Completed += OnDisconnect;
				}
				disconnectAsyncEventArgs.DisconnectReuseSocket = reuseSocket;

				disconnectRequested = true;

				try
				{
					Socket.Shutdown(SocketShutdown.Both);
				}
				catch (Exception)
				{

				}
				Socket.DisconnectAsync(disconnectAsyncEventArgs);
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
					disconnected?.Invoke();
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

				receiveBuffer = null;
				sendBuffer = null;
				recvAsyncEventArgs?.Dispose();
				recvAsyncEventArgs = null;
				connectAsyncEventArgs?.Dispose();
				connectAsyncEventArgs = null;
				disconnectAsyncEventArgs?.Dispose();
				disconnectAsyncEventArgs = null;
				disconnectRequested = false;
				connected = null;

				Socket?.Close();
				Socket = null;
				RemoteEndPoint = null;

				if (IsConnected)
				{
					IsConnected = false;
					OnDisconnected();
					disconnected?.Invoke();
				}

				disconnected = null;
			}
		}

		~TcpClient()
		{
			Dispose(false);
		}
	}
}
