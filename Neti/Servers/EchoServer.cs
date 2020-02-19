using System.Collections.Generic;
using System.Net;
using Neti.Buffer;

namespace Neti.Servers
{
	public class EchoServer : TcpListener
	{
		readonly List<TcpClient> _clients = new List<TcpClient>();

		public EchoServer() : base(IPAddress.Any, 0)
		{

		}

		public EchoServer(int port) : base(IPAddress.Any, port)
		{

		}

		public EchoServer(string ip, int port) : base(ip, port)
		{

		}

		public EchoServer(IPAddress ip, int port) : base(ip, port)
		{
			
		}

		public override void Stop()
		{
			if (IsActive)
			{
				lock (this) _clients.ForEach(client => client.Close());
			}
			base.Stop();
		}

		protected override void OnClientEntered(TcpClient newClient)
		{
			if (newClient is null)
			{
				throw new System.ArgumentNullException(nameof(newClient));
			}

			newClient.BytesReceived += reader => Echo(reader, newClient);
			newClient.Disconnected += () => { lock (this) _clients.Remove(newClient); };
			lock (this) _clients.Add(newClient);
		}

		void Echo(IStreamBufferReader reader, TcpClient client)
		{
			var cloneBuffer = BufferUtility.CloneBuffer(reader.Buffer, reader.ReadPosition, reader.ReadableSize);
			reader.ExternalRead(reader.ReadableSize);
			client.SendAsync(cloneBuffer);
		}
	}
}
