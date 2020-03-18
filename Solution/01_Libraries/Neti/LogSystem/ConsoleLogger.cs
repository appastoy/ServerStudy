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
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine(message);
			Console.ForegroundColor = color;
		}

		public void LogInfo(string message)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Out.WriteLine(message);
			Console.ForegroundColor = color;
		}

		public void LogWarning(string message)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Out.WriteLine(message);
			Console.ForegroundColor = color;
		}
	}
}
