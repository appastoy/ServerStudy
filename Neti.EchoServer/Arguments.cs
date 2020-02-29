namespace Neti.Echo
{
	readonly struct Arguments
	{
		public static bool TryParse(string[] args, out Arguments arguments)
		{
			if (args == null ||
				args.Length < 1 ||
				ushort.TryParse(args[1], out var port) == false ||
				Validator.IsValidPort(port) == false)
			{
				arguments = new Arguments();
				return false;
			}

			arguments = new Arguments(port);
			return true;
		}

		public readonly int Port;

		Arguments(int port)
		{
			Port = port;
		}
	}
}
