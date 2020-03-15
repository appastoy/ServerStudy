using System;

namespace Neti.LogSystem
{
	public sealed class ConsoleLogger : ILogger
	{
		public void Clear()
		{
			Console.Clear();
		}

		public void LogError(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine(message);
		}

		public void LogInfo(string message)
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Out.WriteLine(message);
		}

		public void LogWarning(string message)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Out.WriteLine(message);
		}
	}
}
