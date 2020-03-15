using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neti.CodeGeneration.Generators;
using Neti.Schema;

namespace Neti.CodeGeneration.Builders
{
	public class MessageGroupCodeBuilder : ICodeGenerationContextBuilder
	{
		static readonly ICodeGenerator[] codeGenerators = new ICodeGenerator[]
		{
			new MessageIdCodeGenerator(),
			new RpcCodeGenerator(),
			new MessageHandlingCodeGenerator()
		};

		public IReadOnlyList<CodeGenerationContext> BuildContexts(Assembly assembly)
		{
			var messageGroupTypes = FindMessageGroupTypes(assembly);
			return messageGroupTypes.SelectMany(type => codeGenerators.Select(generator => new CodeGenerationContext(type, generator)))
									.ToArray();
		}

		Type[] FindMessageGroupTypes(Assembly assembly)
		{
			var foundTypes = assembly.GetTypes()
									 .Where(type => type.IsInterface &&
													type.GetCustomAttribute<MessageGroupAttribute>() != null &&
													type.GetMethods().Length > 0)
									 .ToArray();
			return foundTypes;
		}
	}
}
