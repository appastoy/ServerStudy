using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Neti.Echo
{
	public class EchoServer : TcpListener
	{
		readonly List<EchoSession> _sessions = new List<EchoSession>();
		Action<EchoSession> _sessionEntered;

		public event Action<EchoSession> SessionEntered
		{
			add => _sessionEntered += value;
			remove => _sessionEntered -= value;
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

			_sessionEntered?.Invoke(newSession);
		}

		protected override void OnStopped()
		{
			lock (this) _sessions.Clear();
			base.OnStopped();
		}
	}
}
