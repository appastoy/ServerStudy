using System;

namespace Neti.Echo
{
	class Program
	{
		static void Main(string[] args)
		{
			if (Arguments.TryParse(args, out var arguments) == false)
			{
				Console.WriteLine("Invalid arguments.\nUsage: EchoClient {IP} {Port}");
				Environment.Exit(1);
				return;
			}

			using (var echoClient = new EchoClient())
			{
				var serverId = $"{arguments.Ip}:{arguments.Port}";
				echoClient.Connected += () => Console.WriteLine("# Connected! If you want to exit, type \"/exit\".");
				echoClient.MessageReceived += msg => Console.WriteLine($"{serverId} > {msg}");
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
					echoClient.SendMessage(msg);
				}
			}

			Environment.ExitCode = 0;
		}
	}
}
