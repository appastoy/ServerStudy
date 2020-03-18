using System;
using Neti.Echo.Client.MessageHandlers;
using Neti.Packets;

namespace Neti.Echo.Client
{
	public class EchoClient : TcpClient
	{
		static readonly ServerToClientMessageHandler messageHandler = new ServerToClientMessageHandler();

		protected override void OnPacketReceived(PacketReader reader)
		{
			messageHandler.Handle(this, reader);
		}
	}
}
