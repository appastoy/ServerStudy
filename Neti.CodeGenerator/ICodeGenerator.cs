using System;
using System.Threading.Tasks;

namespace Neti.CodeGenerator
{
	public interface ICodeGenerator
	{
		string Generate(Type type);
		Task<string> GenerateAsync(Type type);
	}
}
