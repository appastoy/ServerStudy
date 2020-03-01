using System;
using Neti.Packets;

namespace Neti.Echo
{
	public class EchoClient : TcpClient
	{
		Action<string> _messageReceived;

		public event Action<string> MessageReceived
		{
			add => _messageReceived += value;
			remove => _messageReceived -= value;
		}

		public EchoClient()
		{
			PacketReceived += OnPacketReceived;
		}

		public void SendMessage(string message)
		{
			if (message is null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			using (var writer = CreatePacketWriter())
			{
				writer.Write(message);
			}
			FlushPackets();
		}

		public void SendMessageAsync(string message)
		{
			if (message is null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			using (var writer = CreatePacketWriter())
			{
				writer.Write(message);
			}
			FlushPacketsAsync();
		}

		void OnPacketReceived(in PacketReader reader)
		{
			reader.Reset();
			var message = reader.ReadString();

			_messageReceived?.Invoke(message);
		}

		protected override void Dispose(bool disposing)
		{
			PacketReceived -= OnPacketReceived;
			_messageReceived = null;
			base.Dispose(disposing);
		}
	}
}
