using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Neti.CodeCompilation
{
	public sealed class CSharpCompiler : ICompiler
	{
		public const int MinimumAssemblySize = 1024 * 512;
		static readonly CSharpCompilationOptions compilationOption = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

		public Assembly Compile(string assemblyName, IEnumerable<(string Path, string Code)> codes, IEnumerable<string> assemblyLocations)
		{
			var syntaxTrees = codes.Select(code => CSharpSyntaxTree.ParseText(code.Code, path: code.Path));
			var references = assemblyLocations.Concat(DefaultReferences.Locations)
											  .Distinct()
											  .Select(location => MetadataReference.CreateFromFile(location));
			var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, references, compilationOption);
			
			using (var stream = new MemoryStream(MinimumAssemblySize))
			{
				var result = compilation.Emit(stream);
				if (result.Success)
				{
					stream.Seek(0, SeekOrigin.Begin);

					return Assembly.Load(stream.ToArray());
				}

				var errors = result.Diagnostics.Where(diag => diag.Severity == DiagnosticSeverity.Error).ToArray();

				throw new CompileFailedException(errors);
			}
		}
	}
}
