using System;
using System.Net;
using System.Net.Sockets;

namespace Neti
{
	public class TcpSession : TcpClient
	{
		TcpServer server;

		public readonly int Id;

		public IPEndPoint ServerEndPoint => server?.LocalEndPoint;
		public IPAddress ServerAddress => ServerEndPoint?.Address;
		public int ServerPort => ServerEndPoint.Port;

		public IReadOnlySessionCollection Sessions => server.Sessions;

		public TcpSession(TcpServer server, Socket socket, int id)
		{
			if (socket == null)
			{
				throw new ArgumentNullException(nameof(socket));
			}

			if (socket.Connected == false)
			{
				throw new ArgumentException("socket is not connected.", nameof(socket));
			}

			Id = id;
			this.server = server ?? throw new ArgumentNullException(nameof(server));
			Socket = socket;
			RemoteEndPoint = Socket.RemoteEndPoint as IPEndPoint;
			IsConnected = true;

			BeginRecevie();
		}

		protected override void OnDisconnected()
		{
			base.OnDisconnected();
			server?.Sessions?.Remove(Id);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			server = null;
		}
	}
}
