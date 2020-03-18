using System;
using System.Reflection;
using Neti.CodeCompilation;
using Neti.LogSystem;
using Neti.CodeGeneration.Builders;
using Neti.CodeGeneration;

namespace Neti.MessageGroupCodeGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			Logger.AddLogger(new ConsoleLogger());
			Logger.EnableAutoFlush();

			try
			{
				if (Arguments.TryParse(args) == false)
				{
					Logger.LogError("Usage: MessageCodeGenerator.exe [Source Directory] [Output Directory]");
					Environment.ExitCode = 1;
					return;
				}

				Logger.LogInfo("# Compiling ...");
				var compiler = new CSharpCompiler();
				var compilationResult = compiler.CompileFromDirectory(Arguments.SourceCodeDirectory, references: new Assembly[] { typeof(TcpSession).Assembly });
				if (compilationResult.Success == false)
				{
					Logger.LogError(compilationResult.FullLog);
					Environment.ExitCode = 2;
					return;
				}

				Logger.LogInfo(compilationResult.FullLog);

				Logger.LogInfo("# Generating...");
				var processingResult = CodeGenerationPipeline.Process(compilationResult.Assembly,
																	  Arguments.OutputDirectory,
																	  new ICodeGenerationContextBuilder[] { new MessageGroupCodeBuilder() });
				Logger.LogInfo(processingResult.FullLog);
				Environment.ExitCode = 0;
			}
			finally
			{
				Logger.Flush();
			}
		}
	}
}
