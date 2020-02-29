using System;
using System.Threading;

namespace Neti.Echo
{
	class Program
	{
		static void Main(string[] args)
		{
			if (Arguments.TryParse(args, out var arguments) == false)
			{
				Console.WriteLine("Invalid arguments.\nUsage: EchoServer {Port}");
				Environment.Exit(1);
				return;
			}

			using (var echoServer = new EchoServer())
			{
				echoServer.Started += () => Console.WriteLine("# Server Started.");
				echoServer.Stopped += () => Console.WriteLine("# Server Stopped.");
				echoServer.SessionEntered += session =>
				{
					var clientId = $"{session.Address}:{session.Port}";
					Console.WriteLine($"# Client Entered. ({clientId})");

					session.MessageReceived += msg => Console.WriteLine($"{clientId} > {msg}");
					session.Disconnected += () => Console.WriteLine($"# Client Disconnected. ({clientId})");
				};

				Console.WriteLine($"# Starting server... (Port: {arguments.Port})");
				echoServer.Start(arguments.Port);

				if (echoServer.IsActive == false)
				{
					Console.WriteLine("# Failed to start server.");
					Environment.Exit(2);
					return;
				}
			
				while (echoServer.IsActive)
				{
					Thread.Yield();
				}
			}

			Environment.ExitCode = 0;
		}
	}
}
