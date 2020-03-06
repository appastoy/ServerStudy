using System;

namespace Neti.CodeGenerator
{
	public interface ICodeGenerator
	{
		string Generate(Type type);
	}
}
