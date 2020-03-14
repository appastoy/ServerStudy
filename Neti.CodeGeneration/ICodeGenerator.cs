using System;

namespace Neti.CodeGeneration
{
	public interface ICodeGenerator
	{
		CodeGenerationResult Generate(Type type);
	}
}
