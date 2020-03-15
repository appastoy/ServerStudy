using System.Text.RegularExpressions;

namespace Neti.Echo
{
	readonly struct Arguments
	{
		public static bool TryParse(string[] args, out Arguments arguments)
		{
			if (args == null ||
				args.Length < 2 ||
				Regex.IsMatch(args[0], @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}") == false ||
				ushort.TryParse(args[1], out var port) == false ||
				Validator.IsValidPort(port) == false)
			{
				arguments = new Arguments();
				return false;
			}

			arguments = new Arguments(args[0], port);
			return true;
		}

		public readonly string Ip;
		public readonly int Port;

		Arguments(string ip, int port)
		{
			Ip = ip;
			Port = port;
		}
	}
}
