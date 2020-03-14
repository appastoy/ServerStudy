using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neti.CodeCompilation
{
	public static class DefaultReferences
	{
		public static readonly IReadOnlyList<string> Locations;

		static DefaultReferences()
		{
			var defaultAssemblyLocation = typeof(object).Assembly.Location;
			var defaultAssemblyDirectory = Path.GetDirectoryName(defaultAssemblyLocation);
			Locations = new[]
			{
				defaultAssemblyLocation,
				Path.Combine(defaultAssemblyDirectory, "mscorlib.dll"),
				Path.Combine(defaultAssemblyDirectory, "System.dll"),
				Path.Combine(defaultAssemblyDirectory, "System.Core.dll"),
				Path.Combine(defaultAssemblyDirectory, "System.Runtime.dll")
			}
			.Distinct()
			.ToArray();
		}
	}
}
