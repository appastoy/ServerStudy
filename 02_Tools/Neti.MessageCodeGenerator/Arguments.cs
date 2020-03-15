using System.IO;
using System.Reflection;
using Neti.LogSystem;

namespace Neti.CodeGeneration
{
	static class Arguments
	{
		public static string SourceCodeDirectory { get; private set; }

		public static string OutputDirectory { get; private set; } 
		
		public static bool TryParse(string[] args)
		{
			if (args.Length < 1)
			{
				Logger.LogError("No arguments.");
				return false;
			}

			if (Directory.Exists(args[0]) == false)
			{
				Logger.LogError("SourceCodeDirectory is not exist");
				return false;
			}

			SourceCodeDirectory = args[0];
			if (args.Length > 1)
			{
				if (string.IsNullOrEmpty(args[1]) ||
					Directory.Exists(args[1]) == false)
				{
					Logger.LogError("Can't find Output Directory.");
					return false;
				}

				OutputDirectory = args[1];
			}
			else
			{
				OutputDirectory = ".";
			}

			return true;
		}
	}
}
