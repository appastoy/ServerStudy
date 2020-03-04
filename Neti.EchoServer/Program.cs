using System;
using System.Threading;
using Neti.Echo.Server;
using Neti.LogSystem;

namespace Neti.Echo
{
	class Program
	{
		static void Main(string[] args)
		{
			Logger.AddLogger(new ConsoleLogger());
			Logger.AddLogger(new FileLogger($"{DateTime.Now:yyyyMMdd}_EchoServerLog.txt"));
			Logger.EnableAutoFlush();

			if (Arguments.TryParse(args, out var arguments) == false)
			{
				Logger.LogError("Invalid arguments.\nUsage: Neti.EchoServer {Port}");
				Environment.Exit(1);
				return;
			}

			using (var echoServer = new EchoServer())
			{
				echoServer.Started += () => Logger.LogInfo("# Server Started.");
				echoServer.Stopped += () => Logger.LogInfo("# Server Stopped.");
				echoServer.SessionEntered += session =>
				{
					var clientId = $"{session.RemoteAddress}:{session.RemotePort}";
					Logger.LogInfo($"# Client Entered. ({clientId})");

					session.Disconnected += () => Logger.LogInfo($"# Client Disconnected. ({clientId})");
				};

				Logger.LogInfo($"# Starting server... (Port: {arguments.Port})");
				echoServer.Start(arguments.Port);

				if (echoServer.IsActive == false)
				{
					Logger.LogError("# Failed to start server.");
					Environment.Exit(2);
					return;
				}
			
				while (echoServer.IsActive)
				{
					Thread.Sleep(17);
				}
			}

			Environment.ExitCode = 0;
		}
	}
}
