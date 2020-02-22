using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Neti.Servers
{
	public class EchoServer : TcpListener
	{
		readonly List<EchoSession> _sessions = new List<EchoSession>();

		public override void Stop()
		{
			if (IsActive)
			{
				lock (this) _sessions.ForEach(client => client.Close());
			}
			base.Stop();
		}

		protected override void OnClientEntered(Socket newClientSocket)
		{
			if (newClientSocket is null)
			{
				throw new ArgumentNullException(nameof(newClientSocket));
			}

			var newSession = TcpClient.CreateFromSocket<EchoSession>(newClientSocket);
			newSession.Disconnected += () => { lock (this) _sessions.Remove(newSession); };
			lock (this) _sessions.Add(newSession);
		}

		protected override void OnStopped()
		{
			base.OnStopped();
			_sessions.Clear();
		}
	}
}
