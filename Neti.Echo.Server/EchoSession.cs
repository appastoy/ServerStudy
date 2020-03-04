using System.Net.Sockets;
using Neti.Echo.Server.MessageHandlers;
using Neti.Packets;

namespace Neti.Echo.Server
{
	class EchoSession : TcpSession
	{
		static readonly ClientToServerMessageHandler messageHandler = new ClientToServerMessageHandler();

		public EchoSession(TcpServer server, Socket socket, int id) : base(server, socket, id)
		{

		}

		protected override void OnPacketReceived(PacketReader reader)
		{
			messageHandler.Handle(this, reader);
		}
	}
}
