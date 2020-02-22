using System;
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

		// TODO: Optimize Send.
	}
}
