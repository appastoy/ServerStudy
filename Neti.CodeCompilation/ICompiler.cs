using System.Collections.Generic;
using System.Reflection;

namespace Neti.CodeCompilation
{
	public interface ICompiler
	{
		Assembly Compile(string assemblyName, IEnumerable<(string Path, string Code)> codes, IEnumerable<string> assemblyLocations);
	}
}
