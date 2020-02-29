using System;
using System.Threading;

namespace Neti.Echo
{
	class Program
	{
		static volatile int receiveMessageCheck;

		static void Main(string[] args)
		{
			if (Arguments.TryParse(args, out var arguments) == false)
			{
				Console.WriteLine("Invalid arguments.\nUsage: Neti.EchoClient {IP} {Port}");
				Environment.Exit(1);
				return;
			}

			using (var echoClient = new EchoClient())
			{
				var serverId = $"{arguments.Ip}:{arguments.Port}";
				echoClient.Connected += () => Console.WriteLine("# Connected! If you want to exit, type \"/exit\".");
				echoClient.MessageReceived += msg =>
				{
					Console.WriteLine($"{serverId} > {msg}");
					Interlocked.Exchange(ref receiveMessageCheck, 1);
				};
				echoClient.Disconnected += () => Console.WriteLine("# Disconnected.");

				Console.WriteLine($"# Connecting to {serverId}...");
				echoClient.Connect(arguments.Ip, arguments.Port);

				if (echoClient.IsConnected == false)
				{
					Console.WriteLine("# Failed to conntect.");
					Environment.Exit(2);
					return;
				}

				while (echoClient.IsConnected)
				{
					Console.Write($"{serverId} < ");
					var msg = Console.ReadLine();
					if (msg == "/exit")
					{
						echoClient.Disconnect();
						break;
					}
					Interlocked.Exchange(ref receiveMessageCheck, 0);
					echoClient.SendMessage(msg);
					while (receiveMessageCheck == 0)
					{
						Thread.Yield();
					}
				}
			}

			Environment.ExitCode = 0;
		}
	}
}
