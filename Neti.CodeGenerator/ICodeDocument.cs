using System.Reflection;
using System.Threading.Tasks;

namespace Neti.CodeGenerator
{
	interface ICodeDocument
	{
		public Task GenerateCodeAsync(Assembly assembly);
		public void Export(string outputDirectory);
	}
}
