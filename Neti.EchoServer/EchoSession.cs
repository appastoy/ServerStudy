using System;
using System.Net.Sockets;
using System.Text;
using Neti.Buffer;

namespace Neti.Echo
{
	public class EchoSession : TcpClient
	{
		Action<string> _messageReceived;

		public event Action<string> MessageReceived
		{
			add => _messageReceived += value;
			remove => _messageReceived -= value;
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
					_messageReceived?.Invoke(message);
					OnMessageReceived(message);

					SendAsync(reader.Buffer, reader.ReadPosition, reader.ReadableSize, reader);
				}
			}
		}

		protected override void OnBytesSent(SocketAsyncEventArgs e)
		{
			if (e.UserToken is IStreamBufferReader reader)
			{
				reader.ExternalRead(reader.ReadableSize);
			}
			e.UserToken = null;
		}

		protected virtual void OnMessageReceived(string message)
		{
			
		}
	}
}
