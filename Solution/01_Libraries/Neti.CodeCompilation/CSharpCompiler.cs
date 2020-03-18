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

		public CompilationResult Compile(string assemblyName, IEnumerable<CodeFile> codeFiles, IEnumerable<string> assemblyLocations)
		{
			var syntaxTrees = codeFiles.Select(code => CSharpSyntaxTree.ParseText(code.Code, path: code.Path));
			var references = assemblyLocations.Concat(DefaultReferences.Locations)
											  .Distinct()
											  .Select(location => MetadataReference.CreateFromFile(location));
			var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, references, compilationOption);
			
			using (var stream = new MemoryStream(MinimumAssemblySize))
			{
				Assembly assembly = null;
				var result = compilation.Emit(stream);
				if (result.Success)
				{
					stream.Seek(0, SeekOrigin.Begin);

					assembly = Assembly.Load(stream.ToArray());
				}

				return new CompilationResult(result, assembly);
			}
		}
	}
}
