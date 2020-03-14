using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace Neti.CodeCompilation.Tests
{
	public class CSharpCompilerTests
	{
		[Test]
		public void InvalidCodeCompileTest()
		{
			var invalidCode = "This is not valid C# code.";

			var compiler = new CSharpCompiler();
			new Action(() => compiler.CompileCode(invalidCode)).Should().Throw<CompileFailedException>();
		}

		[Test]
		public void ValidCodeCompileTest()
		{
			var validCode = 
@"using System;

namespace Test
{
	public class Program
	{
		public static int Plus(int a, int b)
		{
			return a + b;
		}
	}
}";

			var compiler = new CSharpCompiler();
			var assembly = compiler.CompileCode(validCode);
			assembly.Should().NotBeNull();
			var programClass = assembly.GetTypes().FirstOrDefault(type => type.Name == "Program");
			programClass.Should().NotBeNull();
			var plusMethod = programClass.GetMethod("Plus", BindingFlags.Public | BindingFlags.Static);
			plusMethod.Should().NotBeNull();
			plusMethod.Invoke(null, new object[] { 1, 2 }).As<int>().Should().Be(3);
		}
	}
}