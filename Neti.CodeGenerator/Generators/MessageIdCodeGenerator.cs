using System;
using System.Linq;

namespace Neti.CodeGenerator.Generators
{
	public class MessageIdCodeGenerator : ICodeGenerator
	{
		public string Generate(Type type)
		{
			var messageGroup = TypeUtility.GetMessageGroupAttribute(type);
			var startId = messageGroup.StartId;
			var messageIds = type.GetMethods()
								 .Select(method => (method.Name, Value: startId++))
								 .ToArray();

			return GenerateMessageIdCode(type, messageIds);
		}

		string GenerateMessageIdCode(Type messageGroupType, (string Name, ushort Value)[] messageIds)
		{
			var messageIdDeclaringCodes = messageIds.Select(id => $"public const ushort {id.Name} = {id.Value};");
			var messageIdCode = string.Join($"{Environment.NewLine}{CodeConstants.MessageCodeIndent}", messageIdDeclaringCodes);

			return CodeUtility.BuildMessageGroupCode(string.Empty,
													 messageGroupType.Namespace,
													 messageGroupType.Name,
													 "MessageId",
													 messageIdCode);
		}
	}
}
