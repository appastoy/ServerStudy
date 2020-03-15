using System.Collections.Generic;

namespace Neti.CodeCompilation
{
	public interface ICompiler
	{
		CompilationResult Compile(string assemblyName, IEnumerable<CodeFile> codeFiles, IEnumerable<string> assemblyLocations);
	}
}
