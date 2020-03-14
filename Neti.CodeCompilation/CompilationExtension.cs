using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Neti.IO;

namespace Neti.CodeCompilation
{
	public static class CompilationExtension
	{
		public static Assembly CompileFromFile(this ICompiler compiler, string path)
		{
			return CompileFromFiles(compiler, new string[] { path });
		}

		public static Assembly CompileFromFile(this ICompiler compiler, string path, IEnumerable<string> assemblyLocations)
		{
			return CompileFromFiles(compiler, new string[] { path }, assemblyLocations);
		}

		public static Assembly CompileFromFile(this ICompiler compiler, string path, IEnumerable<Assembly> references)
		{
			return CompileFromFiles(compiler, new string[] { path }, AssemblyHelper.CollectAssemblyLocations(references));
		}

		public static Assembly CompileFromFile(this ICompiler compiler, string assemblyName, string path)
		{
			return CompileFromFiles(compiler, assemblyName, new string[] { path });
		}

		public static Assembly CompileFromFile(this ICompiler compiler, string assemblyName, string path, IEnumerable<string> assemblyLocations)
		{
			return CompileFromFiles(compiler, assemblyName, new string[] { path }, assemblyLocations);
		}

		public static Assembly CompileFromFile(this ICompiler compiler, string assemblyName, string path, IEnumerable<Assembly> references)
		{
			return CompileFromFiles(compiler, assemblyName, new string[] { path }, AssemblyHelper.CollectAssemblyLocations(references));
		}

		public static Assembly CompileFromFiles(this ICompiler compiler, IEnumerable<string> pathList)
		{
			return CompileFromFiles(compiler, pathList, DefaultReferences.Locations);
		}

		public static Assembly CompileFromFiles(this ICompiler compiler, IEnumerable<string> pathList, IEnumerable<string> assemblyLocations)
		{
			return CompileFromFiles(compiler, Guid.NewGuid().ToString(), pathList, assemblyLocations);
		}

		public static Assembly CompileFromFiles(this ICompiler compiler, IEnumerable<string> pathList, IEnumerable<Assembly> references)
		{
			return CompileFromFiles(compiler, Guid.NewGuid().ToString(), pathList, AssemblyHelper.CollectAssemblyLocations(references));
		}

		public static Assembly CompileFromFiles(this ICompiler compiler, string assemblyName, IEnumerable<string> pathList)
		{
			return CompileFromFiles(compiler, assemblyName, pathList, DefaultReferences.Locations);
		}

		public static Assembly CompileFromFiles(this ICompiler compiler, string assemblyName, IEnumerable<string> pathList, IEnumerable<string> assemblyLocations)
		{
			var fileReadTasks = pathList.Distinct().Select(FileUtility.ReadAllTextAsync).ToArray();
			Task.WaitAll(fileReadTasks);
			var codes = pathList.Select((path, i) => (path, fileReadTasks[i].Result));

			return compiler.Compile(assemblyName, codes, assemblyLocations);
		}

		public static Assembly CompileFromFiles(this ICompiler compiler, string assemblyName, IEnumerable<string> pathList, IEnumerable<Assembly> references)
		{
			return CompileFromFiles(compiler, assemblyName, pathList, AssemblyHelper.CollectAssemblyLocations(references));
		}

		//-----------------------------------------------------------------------------------------------------------

		public static Assembly CompileFromDirectory(this ICompiler compiler, string directory)
		{
			return CompileFromDirectory(compiler, directory, DefaultReferences.Locations);
		}

		public static Assembly CompileFromDirectory(this ICompiler compiler, string directory, IEnumerable<string> assemblyLocations)
		{
			return CompileFromDirectories(compiler, new string[] { directory }, assemblyLocations);
		}

		public static Assembly CompileFromDirectory(this ICompiler compiler, string directory, IEnumerable<Assembly> references)
		{
			return CompileFromDirectories(compiler, new string[] { directory }, AssemblyHelper.CollectAssemblyLocations(references));
		}

		public static Assembly CompileFromDirectory(this ICompiler compiler, string assemblyName, string directory)
		{
			return CompileFromDirectory(compiler, assemblyName, directory, DefaultReferences.Locations);
		}

		public static Assembly CompileFromDirectory(this ICompiler compiler, string assemblyName, string directory, IEnumerable<string> assemblyLocations)
		{
			return CompileFromDirectories(compiler, assemblyName, new string[] { directory }, assemblyLocations);
		}

		public static Assembly CompileFromDirectory(this ICompiler compiler, string assemblyName, string directory, IEnumerable<Assembly> references)
		{
			return CompileFromDirectories(compiler, assemblyName, new string[] { directory }, AssemblyHelper.CollectAssemblyLocations(references));
		}

		public static Assembly CompileFromDirectories(this ICompiler compiler, IEnumerable<string> directories)
		{
			return CompileFromDirectories(compiler, directories, DefaultReferences.Locations);
		}

		public static Assembly CompileFromDirectories(this ICompiler compiler, IEnumerable<string> directories, IEnumerable<string> assemblyLocations)
		{
			return CompileFromDirectories(compiler, Guid.NewGuid().ToString(), directories, assemblyLocations);
		}

		public static Assembly CompileFromDirectories(this ICompiler compiler, IEnumerable<string> directories, IEnumerable<Assembly> references)
		{
			return CompileFromDirectories(compiler, Guid.NewGuid().ToString(), directories, AssemblyHelper.CollectAssemblyLocations(references));
		}

		public static Assembly CompileFromDirectories(this ICompiler compiler, string assemblyName, IEnumerable<string> directories)
		{
			return CompileFromDirectories(compiler, assemblyName, directories, DefaultReferences.Locations);
		}

		public static Assembly CompileFromDirectories(this ICompiler compiler, string assemblyName, IEnumerable<string> directories, IEnumerable<string> assemblyLocations)
		{
			var pathList = directories.Distinct().SelectMany(directory => Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories));

			return CompileFromFiles(compiler, assemblyName, pathList, assemblyLocations);
		}

		public static Assembly CompileFromDirectories(this ICompiler compiler, string assemblyName, IEnumerable<string> directories, IEnumerable<Assembly> references)
		{
			return CompileFromDirectories(compiler, assemblyName, directories, AssemblyHelper.CollectAssemblyLocations(references));
		}

		//-----------------------------------------------------------------------------------------------------------

		public static Assembly CompileCode(this ICompiler compiler, string code)
		{
			return CompileCodes(compiler, new string[] { code });
		}

		public static Assembly CompileCode(this ICompiler compiler, string code, IEnumerable<string> assemblyLocations)
		{
			return CompileCodes(compiler, new string[] { code }, assemblyLocations);
		}

		public static Assembly CompileCode(this ICompiler compiler, string code, IEnumerable<Assembly> references)
		{
			return CompileCodes(compiler, new string[] { code }, AssemblyHelper.CollectAssemblyLocations(references));
		}

		public static Assembly CompileCode(this ICompiler compiler, string assemblyName, string code)
		{
			return CompileCodes(compiler, assemblyName, new string[] { code });
		}

		public static Assembly CompileCode(this ICompiler compiler, string assemblyName, string code, IEnumerable<string> assemblyLocations)
		{
			return compiler.CompileCodes(assemblyName, new string[] { code }, assemblyLocations);
		}

		public static Assembly CompileCode(this ICompiler compiler, string assemblyName, string code, IEnumerable<Assembly> references)
		{
			return compiler.CompileCodes(assemblyName, new string[] { code }, AssemblyHelper.CollectAssemblyLocations(references));
		}

		public static Assembly CompileCodes(this ICompiler compiler, IEnumerable<string> codes)
		{
			return CompileCodes(compiler, codes, DefaultReferences.Locations);
		}

		public static Assembly CompileCodes(this ICompiler compiler, IEnumerable<string> codes, IEnumerable<string> assemblyLocations)
		{
			return compiler.CompileCodes(Guid.NewGuid().ToString(), codes, assemblyLocations);
		}

		public static Assembly CompileCodes(this ICompiler compiler, IEnumerable<string> codes, IEnumerable<Assembly> references)
		{
			return compiler.CompileCodes(Guid.NewGuid().ToString(), codes, AssemblyHelper.CollectAssemblyLocations(references));
		}

		public static Assembly CompileCodes(this ICompiler compiler, string assemblyName, IEnumerable<string> codes)
		{
			return compiler.CompileCodes(assemblyName, codes, DefaultReferences.Locations);
		}

		public static Assembly CompileCodes(this ICompiler compiler, string assemblyName, IEnumerable<string> codes, IEnumerable<string> assemblyLocations)
		{
			var pathCodes = codes.Select((code, i) => ($"Code_{i+1:00}.cs", code));

			return compiler.Compile(assemblyName, pathCodes, assemblyLocations);
		}

		public static Assembly CompileCodes(this ICompiler compiler, string assemblyName, IEnumerable<string> codes, IEnumerable<Assembly> references)
		{
			return compiler.CompileCodes(assemblyName, codes, AssemblyHelper.CollectAssemblyLocations(references));
		}
	}
}
