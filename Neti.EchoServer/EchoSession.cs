using System;
using System.Net.Sockets;
using Neti.Buffer;
using Neti.Packets;

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

		public EchoSession()
		{
			PacketReceived += OnPacketReceived;
		}

		void OnPacketReceived(in PacketReader reader)
		{
			reader.Reset();
			var message = reader.ReadString();
			_messageReceived?.Invoke(message);

			EchoMessage(message);
		}

		void EchoMessage(string message)
		{
			using (var writer = CreatePacketWriter())
			{
				writer.Write(message);
			}
			FlushPacketsAsync();
		}

		protected override void Dispose(bool disposing)
		{
			_messageReceived = null;
			PacketReceived -= OnPacketReceived;
			base.Dispose(disposing);
		}
	}
}
