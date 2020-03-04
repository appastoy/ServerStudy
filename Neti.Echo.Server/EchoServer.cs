using System.Net.Sockets;

namespace Neti.Echo.Server
{
	public class EchoServer : TcpServer
	{
		protected override TcpSession CreateSession(Socket socket, int id)
		{
			return new EchoSession(this, socket, id);
		}
	}
}
