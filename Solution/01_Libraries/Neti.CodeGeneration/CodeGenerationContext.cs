using System;

namespace Neti.CodeGeneration
{
	public readonly struct CodeGenerationContext
	{
		public readonly Type Type;
		public readonly ICodeGenerator Generator;

		public CodeGenerationContext(Type type, ICodeGenerator generator)
		{
			Type = type ?? throw new ArgumentNullException(nameof(type));
			Generator = generator ?? throw new ArgumentNullException(nameof(generator));
		}
	}
}
