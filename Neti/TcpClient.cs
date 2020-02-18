using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Neti.Buffer;
using Neti.Pool;

namespace Neti
{
	public class TcpClient : IDisposable
	{
		IPEndPoint _remoteEndPoint;
        Action _connected;
		Action _disconnected;
		Action<IStreamBufferReader> _bytesReceived;
		SocketAsyncEventArgs _connectAsyncEventArgs;
		SocketAsyncEventArgs _disconnectAsyncEventArgs;
		SocketAsyncEventArgs _recvAsyncEventArgs;
		bool _disconnectRequired;

		public Socket Socket { get; private set; }
		public IPAddress Address => _remoteEndPoint.Address;
		public int Port => _remoteEndPoint.Port;
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

		public event Action<IStreamBufferReader> BytesReceived
		{
			add => _bytesReceived += value;
			remove => _bytesReceived -= value;
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

			Validator.ValidatePort(port);
			_remoteEndPoint = new IPEndPoint(ipAddress, port);
			EnsureSocket();
		}

		public TcpClient(IPAddress ip, int port)
		{
			if (ip is null)
			{
				throw new ArgumentNullException(nameof(ip));
			}

			Validator.ValidatePort(port);
			_remoteEndPoint = new IPEndPoint(ip, port);
			EnsureSocket();
		}

		public TcpClient(IPEndPoint remoteEndPoint)
		{
			if (remoteEndPoint is null)
			{
				throw new ArgumentNullException(nameof(remoteEndPoint));
			}

			Validator.ValidatePort(remoteEndPoint.Port);
			_remoteEndPoint = remoteEndPoint;
			EnsureSocket();
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
			IsConnected = true;
			BeginRecevie();
		}

		public void Connect()
		{
			CheckDisposed();
			CheckConnected();

			Socket.Connect(_remoteEndPoint);
			IsConnected = true;
			_connected?.Invoke();
			BeginRecevie();
		}

		public void ConnectAsync()
		{
			CheckDisposed();
			CheckConnected();

			if (_connectAsyncEventArgs == null)
			{
				_connectAsyncEventArgs = new SocketAsyncEventArgs { RemoteEndPoint = _remoteEndPoint };
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

			Socket.Send(bytes, offset, count, SocketFlags.None, out var errorCode);
		}

		public void SendAsync(byte[] bytes)
		{
			SendAsync(bytes, 0, bytes != null ? bytes.Length : 0);
		}

		public void SendAsync(byte[] bytes, int offset, int count)
		{
			Validator.ValidateBytes(bytes, offset, count);

			// TODO: EventArgs Pool 구현하기
			var sendAsyncEventArgs = SendAsyncEventArgsPool.Instance.Alloc();
			sendAsyncEventArgs.SetBuffer(bytes, offset, count);
			
			if (Socket.SendAsync(sendAsyncEventArgs) == false)
			{
				SendAsyncEventArgsPool.Instance.Free(sendAsyncEventArgs);
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
			Dispose(true);
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
				Socket = new Socket(_remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
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
				_connected?.Invoke();
				BeginRecevie();
			}
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
			if (Socket.ReceiveAsync(_recvAsyncEventArgs) == false)
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
					if (_bytesReceived != null)
					{
						var streamBuffer = (StreamBuffer)e.UserToken;
						streamBuffer.ExternalWrite(e.BytesTransferred);

						_bytesReceived.Invoke(streamBuffer);
						_recvAsyncEventArgs.SetBuffer(streamBuffer.Buffer, 
													  streamBuffer.WritePosition,
													  streamBuffer.WritableSize);
					}
					else
					{
						e.SetBuffer(0, e.Buffer.Length);
					}

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
					_disconnected?.Invoke();
				}
				else
				{
					Close();
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (IsDisposed == false)
			{
				if (IsConnected)
				{
					IsConnected = false;
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
			}
		}

		~TcpClient()
		{
			Dispose(false);
		}
	}
}
