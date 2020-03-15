using System.Collections.Generic;
using System.Reflection;

namespace Neti.CodeGeneration
{
	public interface ICodeGenerationContextBuilder
	{
		IReadOnlyList<CodeGenerationContext> BuildContexts(Assembly assembly);
	}
}
