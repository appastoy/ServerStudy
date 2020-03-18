using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Neti.CodeCompilation
{
	static class AssemblyHelper
	{
		static Dictionary<AssemblyName, string> assemblyLocationCache = new Dictionary<AssemblyName, string>();

		public static IEnumerable<string> ConvertToLocations(Assembly assembly)
		{
			return assembly.GetReferencedAssemblies()
						   .Select(ConvertToLocation)
						   .Append(assembly.Location);
		}

		public static IEnumerable<string> CollectAssemblyLocations(IEnumerable<Assembly> assemblies)
		{
			return assemblies.SelectMany(ConvertToLocations);
		}

		public static string ConvertToLocation(AssemblyName assemblyName)
		{
			if (assemblyLocationCache.TryGetValue(assemblyName, out var location) == false)
			{
				location = GetAssemblyLocation(assemblyName);
				assemblyLocationCache.Add(assemblyName, location);
			}

			return location;
		}

		static string GetAssemblyLocation(AssemblyName assemblyName)
		{
			string location;
			const string fileUrlPrefix = "file:///";
			if (string.IsNullOrEmpty(assemblyName.CodeBase) == false &&
				assemblyName.CodeBase.StartsWith(fileUrlPrefix))
			{
				location = assemblyName.CodeBase.Substring(fileUrlPrefix.Length).Replace('/', Path.PathSeparator);
			}
			else
			{
				try
				{
					location = Assembly.ReflectionOnlyLoad(assemblyName.FullName).Location;
				}
				catch (PlatformNotSupportedException)
				{
					location = Assembly.Load(assemblyName).Location;
				}
			}

			return location;
		}
	}
}
