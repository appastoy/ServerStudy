using System;
using System.IO;
using System.Linq;

namespace Neti.CodeGeneration.Generators
{
	public class MessageIdCodeGenerator : ICodeGenerator
	{
		const string localPathPostfix = ".MessageId.g.cs";

		public CodeGenerationResult Generate(Type type)
		{
			var messageGroup = TypeUtility.GetMessageGroupAttribute(type);
			var startId = messageGroup.StartId;
			var messageIds = type.GetMethods()
								 .Select(method => (method.Name, Value: startId++))
								 .ToArray();

			var localPath = $"{type.Name.TrimStart('I')}{localPathPostfix}";
			var code = GenerateMessageIdCode(type, messageIds);

			return new CodeGenerationResult(localPath, code);
		}

		string GenerateMessageIdCode(Type messageGroupType, (string Name, ushort Value)[] messageIds)
		{
			var messageIdDeclaringCodes = messageIds.Select(id => $"public const ushort {id.Name} = {id.Value};");
			var messageIdCode = string.Join($"{Environment.NewLine}{CodeConstants.InternalClassCodeIndent}", messageIdDeclaringCodes);

			return CodeGenerationUtility.BuildMessageGroupCode(string.Empty,
															   messageGroupType.Namespace,
															   messageGroupType.Name.TrimStart('I'),
															   "static class MessageId",
															   messageIdCode);
		}
	}
}
