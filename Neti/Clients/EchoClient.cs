using System;
using System.Net.Sockets;
using System.Text;
using Neti.Buffer;
using Neti.Pool;

namespace Neti.Clients
{
	public class EchoClient : TcpClient
	{
		Action<string> _messageReceived;
		byte[] _emtpyMessageBytes = new byte[2];
		GenericPool<StreamBuffer> _bufferPool;

		public event Action<string> MessageReceived
		{
			add => _messageReceived += value;
			remove => _messageReceived -= value;
		}

		public void SendMessage(string message)
		{
			if (message is null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (message.Length == 0)
			{
				Send(_emtpyMessageBytes);
			}
			else
			{
				var buffer = BuildMessageBuffer(message);
				Send(buffer.Buffer, buffer.ReadPosition, buffer.ReadableSize);
				_bufferPool.Free(buffer);
			}
		}

		public void SendMessageAsync(string message)
		{
			if (message is null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (message.Length == 0)
			{
				SendAsync(_emtpyMessageBytes);
			}
			else
			{
				var buffer = BuildMessageBuffer(message);
				SendAsync(buffer.Buffer, buffer.ReadPosition, buffer.ReadableSize, buffer);
			}
		}

		protected override void OnBytesReceived(IStreamBufferReader reader)
		{
			if (reader.ReadableSize >= 2)
			{
				var size = reader.Peek<ushort>();
				var totalSize = size + 2;
				if (reader.ReadableSize >= totalSize)
				{
					var message = Encoding.UTF8.GetString(reader.Buffer, reader.ReadPosition + 2, size);
					reader.ExternalRead(totalSize);

					_messageReceived?.Invoke(message);
				}
			}
		}

		protected override void OnBytesSent(SocketAsyncEventArgs e)
		{
			var buffer = e.UserToken as StreamBuffer ?? throw new ArgumentException("e.UserToken is not a StreamBuffer.");
			_bufferPool.Free(buffer);
		}

		StreamBuffer BuildMessageBuffer(string message)
		{
			var messageBytes = Encoding.UTF8.GetBytes(message);
			if (messageBytes.Length > ushort.MaxValue)
			{
				throw new ArgumentException($"Message is too long. Max length is {ushort.MaxValue:N0}.");
			}

			if (_bufferPool == null)
			{
				_bufferPool = new GenericPool<StreamBuffer>();
			}

			var buffer = _bufferPool.Alloc();
			buffer.Write((ushort)messageBytes.Length);
			buffer.Write(messageBytes);

			return buffer;
		}
	}
}
