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
			compiler.CompileCode(invalidCode).Success.Should().BeFalse();
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
			var result = compiler.CompileCode(validCode);
			result.Success.Should().BeTrue();

			var programClass = result.Assembly.GetTypes().FirstOrDefault(type => type.Name == "Program");
			programClass.Should().NotBeNull();

			var plusMethod = programClass.GetMethod("Plus", BindingFlags.Public | BindingFlags.Static);
			plusMethod.Should().NotBeNull();
			plusMethod.Invoke(null, new object[] { 1, 2 }).As<int>().Should().Be(3);
		}
	}
}