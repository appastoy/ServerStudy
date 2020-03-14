using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Neti.CodeCompilation
{
	static class AssemblyHelper
	{
		public static IEnumerable<string> ConvertToLocations(Assembly assembly)
		{
			return assembly.GetReferencedAssemblies()
						   .Select(Assembly.Load)
						   .Select(refAssembly => refAssembly.Location)
						   .Append(assembly.Location);
		}

		public static IEnumerable<string> CollectAssemblyLocations(IEnumerable<Assembly> assemblies)
		{
			return assemblies.SelectMany(ConvertToLocations);
		}
	}
}
