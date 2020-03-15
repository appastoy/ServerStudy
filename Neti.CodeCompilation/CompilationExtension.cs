using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Neti.IO;

namespace Neti.CodeCompilation
{
	public static class CompilationExtension
	{
		public static CompilationResult CompileCode(this ICompiler compiler,
										   string code,
										   string assemblyName = null,
										   IEnumerable<string> referenceLocations = null,
										   IEnumerable<Assembly> references = null)
		{
			var codeFile = new CodeFile("UnnamedCode.cs", code);

			return CompileInternal(compiler, new CodeFile[] { codeFile }, assemblyName, referenceLocations, references);
		}

		public static CompilationResult CompileCodes(this ICompiler compiler,
										    IEnumerable<string> codes,
										    string assemblyName = null,
										    IEnumerable<string> referenceLocations = null,
										    IEnumerable<Assembly> references = null)
		{
			var codeFiles = codes.Select((code, i) => new CodeFile($"UnnamedCode_{i+1:00}.cs", code));

			return CompileInternal(compiler, codeFiles, assemblyName, referenceLocations, references);
		}

		public static CompilationResult CompileFromFile(this ICompiler compiler, 
											   string path,
											   string assemblyName = null,
											   IEnumerable<string> referenceLocations = null, 
											   IEnumerable<Assembly> references = null, 
											   Encoding encoding = null)
		{
			var validEncoding = encoding ?? Encoding.Default;
			var codeFile = new CodeFile(path, File.ReadAllText(path, validEncoding));

			return CompileInternal(compiler, new CodeFile[] { codeFile }, assemblyName, referenceLocations, references);
		}

		public static CompilationResult CompileFromFiles(this ICompiler compiler, 
												IEnumerable<string> paths,
												string assemblyName = null,
												IEnumerable<string> referenceLocations = null,
												IEnumerable<Assembly> references = null,
												Encoding encoding = null)
		{
			var validEncoding = encoding ?? Encoding.Default;
			var fileReadTasks = paths.Distinct().Select(path => FileUtility.ReadAllTextAsync(path, validEncoding)).ToArray();
			Task.WaitAll(fileReadTasks);
			var codeFiles = paths.Select((path, i) => new CodeFile(path, fileReadTasks[i].Result));

			return CompileInternal(compiler, codeFiles, assemblyName, referenceLocations, references);
		}

		public static CompilationResult CompileFromDirectory(this ICompiler compiler,
															 string directory,
															 string assemblyName = null,
															 IEnumerable<string> referenceLocations = null,
															 IEnumerable<Assembly> references = null,
															 SearchOption searchOption = SearchOption.AllDirectories,
															 Encoding encoding = null)
		{
			var paths = Directory.GetFiles(directory, "*.cs", searchOption);

			return CompileFromFiles(compiler, paths, assemblyName, referenceLocations, references, encoding);
		}

		public static CompilationResult CompileFromDirectories(this ICompiler compiler,
													  IEnumerable<string> directories,
													  string assemblyName = null,
													  IEnumerable<string> referenceLocations = null,
													  IEnumerable<Assembly> references = null,
													  SearchOption searchOption = SearchOption.AllDirectories,
													  Encoding encoding = null)
		{
			var paths = directories.Distinct().SelectMany(directory => Directory.GetFiles(directory, "*.cs", searchOption));

			return CompileFromFiles(compiler, paths, assemblyName, referenceLocations, references, encoding);
		}

		static CompilationResult CompileInternal(ICompiler compiler,
												 IEnumerable<CodeFile> codeFiles,
												 string assemblyName,
												 IEnumerable<string> referenceLocations,
												 IEnumerable<Assembly> references)
		{
			var validAssemblyName = string.IsNullOrEmpty(assemblyName) ? Guid.NewGuid().ToString() : assemblyName;
			var validReferenceLocations = referenceLocations ?? Array.Empty<string>();
			var additionalReferenceLocations = AssemblyHelper.CollectAssemblyLocations(references ?? Array.Empty<Assembly>());

			return compiler.Compile(validAssemblyName, codeFiles, validReferenceLocations.Concat(additionalReferenceLocations));
		}
	}
}
