using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Neti.IO;

namespace Neti.CodeGeneration
{
	public static class CodeGenerationPipeline
	{
		public static void Run(Assembly assembly, string outputDirectory, IEnumerable<ICodeGenerationContextBuilder> builders, Encoding encoding = null)
		{
			if (outputDirectory is null)
			{
				throw new ArgumentNullException(nameof(outputDirectory));
			}

			var codeGenerationContexts = builders.SelectMany(builder => builder.BuildContexts(assembly)).ToArray();
			var codeResults = Generate(codeGenerationContexts);
			UpdateAndRemoveUnusedCode(outputDirectory, codeResults, encoding ?? Encoding.Default);
		}

		public static CodeGenerationResult[] Generate(IReadOnlyList<CodeGenerationContext> codeGenerationContexts)
		{
			var input = new ConcurrentStack<CodeGenerationContext>(codeGenerationContexts);
			var output = new ConcurrentStack<CodeGenerationResult>();

			var taskCount = Math.Min(codeGenerationContexts.Count, Environment.ProcessorCount * 2);
			var tasks = Enumerable.Repeat(Task.Run(() =>
			{
				CodeGenerationContext context;
				while (input.TryPop(out context))
				{
					var result = context.Generator.Generate(context.Type);
					output.Push(result);
				}
			}), taskCount).ToArray();
			Task.WaitAll(tasks);

			return output.ToArray();
		}

		static void UpdateAndRemoveUnusedCode(string outputDirectory, IReadOnlyList<CodeGenerationResult> codeResults, Encoding encoding)
		{
			var fileContentMap = CreateFileContentMap(outputDirectory, encoding);
			var needExportCodes = codeResults.Where(codeResult => fileContentMap.TryGetValue(codeResult.LocalPath, out var content) == false || codeResult.Code != content);
			var needDeleteFiles = fileContentMap.Keys.Except(needExportCodes.Select(codeResult => codeResult.LocalPath)).ToList();

			// Export
			var exportCodeTasks = needExportCodes.Select(codeResult =>
			{
				var filePath = Path.Combine(outputDirectory, codeResult.LocalPath);
				return FileUtility.WriteAllTextAsync(filePath, codeResult.Code, encoding);
			})
				.ToArray();
			Task.WaitAll(exportCodeTasks);

			// Remove
			needDeleteFiles.ForEach(localPath =>
			{
				var filePath = Path.Combine(outputDirectory, localPath);
				File.Delete(filePath);
			});
		}

		static Dictionary<string, string> CreateFileContentMap(string outputDirectory, Encoding encoding)
		{
			var filePathes = Directory.GetFiles(outputDirectory);
			var fileLoadTasks = filePathes.Select(path => FileUtility.ReadAllTextAsync(path, encoding)).ToArray();
			Task.WaitAll(fileLoadTasks);

			var fileContentMap = new Dictionary<string, string>(filePathes.Length);
			for (int i = 0; i < filePathes.Length; i++)
			{
				var localPath = filePathes[i].Substring(outputDirectory.Length + 1).ResolvePathSeperator();
				fileContentMap.Add(localPath, fileLoadTasks[i].Result);
			}

			return fileContentMap;
		}
	}
}
