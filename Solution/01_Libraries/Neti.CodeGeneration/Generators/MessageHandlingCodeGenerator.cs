﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Neti.CodeGeneration.Generators
{
	public class MessageHandlingCodeGenerator : ICodeGenerator
	{
		const string localPathPostFix = ".MessageHandling.g.cs";

		public CodeGenerationResult Generate(Type type)
		{
			var messageGroup = TypeUtility.GetMessageGroupAttribute(type);
			string senderTypeName = TypeUtility.GetHandlerSenderTypeName(messageGroup);

			var localPath = $"{type.Name.TrimStart('I')}{localPathPostFix}";
			var code = GenerateCompositedCode(type, senderTypeName, type.GetMethods());

			return new CodeGenerationResult(localPath, code);
		}

		string GenerateCompositedCode(Type messageGroupType, string senderTypeName, IEnumerable<MethodInfo> methods)
		{
			var usingCode = TypeUtility.GetUnfriendlyParameterTypeUsingCode(methods, messageGroupType.Namespace, new[] { "Neti.Packets" });
			var messageHandlingCode = GenerateMessageHandlingCode(senderTypeName, methods);

			return CodeGenerationUtility.BuildMessageGroupCode(usingCode,
															   messageGroupType.Namespace,
															   messageGroupType.Name.TrimStart('I'),
															   "abstract class MessageHandling",
															   messageHandlingCode);
		}

		string GenerateMessageHandlingCode(string senderTypeName, IEnumerable<MethodInfo> methods)
		{
			var handlerDefinitionCode = GenerateHandlerDefinitionCode(senderTypeName, methods);
			var handleMethodCode = GenerateHandleMethodCode(senderTypeName, methods);
			var handleMessageMethodCode = GenerateHandleMessageMethodCode(senderTypeName, methods);
			var abstractMessageMethodCode = GenerateAbstractMessageMethodCode(senderTypeName, methods);

			var messageHandlingCode =
$@"{handlerDefinitionCode}

{handleMethodCode}

{handleMessageMethodCode}

{abstractMessageMethodCode}".InsertAfterEachLine(CodeConstants.InternalClassCodeIndent);

			return messageHandlingCode;
		}

		string GenerateHandlerDefinitionCode(string senderTypeName, IEnumerable<MethodInfo> methods)
		{
			var delegateCode = methods.Select(method =>
			{
				var paramCode = CodeGenerationUtility.GenerateParameterTypeNameCode(method.GetParameters());

				return $"public delegate void {method.Name}Handler({senderTypeName} sender{paramCode});";
			})
			.JoinWithLine();
			var declaringCode = methods.Select(method => $"{method.Name}Handler on{method.Name};").JoinWithLine();
			var eventCode = methods.Select(method => $"public event {method.Name}Handler On{method.Name} {{ add {{ on{method.Name} += value; }} remove {{ on{method.Name} -= value; }} }}").JoinWithLine();

			var code =
$@"{delegateCode}

{declaringCode}

{eventCode}";

			return code;
		}

		string GenerateHandleMethodCode(string senderTypeName, IEnumerable<MethodInfo> methods)
		{
			var caseCodes = methods.Select(method => $"case MessageId.{method.Name}: Handle{method.Name}(sender, reader); break;");
			var caseCode = string.Join($"{Environment.NewLine}			", caseCodes);

			var code = 
$@"public void Handle({senderTypeName} sender, PacketReader reader)
{{
	try
	{{
		reader.Reset();
		
		var messageId = reader.Read<ushort>();
		switch (messageId)
		{{
			{caseCode}
			
			default: throw new UnknownMessageException(messageId);
		}}
	}}
	finally
	{{
		reader.Use();
	}}
}}";

			return code;
		}

		string GenerateHandleMessageMethodCode(string senderTypeName, IEnumerable<MethodInfo> methods)
		{
			var handleMessageCode = methods.Select(method =>
			{
				var parameters = method.GetParameters();
				var readMessageDataCode = parameters.Select(param =>
				{
					var postfix = TypeUtility.GetPacketReaderReadMethodPostfix(param.ParameterType);
					return $"var {param.Name} = reader.Read{postfix}();";
				})
				.Join($"{Environment.NewLine}	");

				var paramNamesCode = CodeGenerationUtility.GenerateParameterNameCode(parameters);

				var code =
$@"void Handle{method.Name}({senderTypeName} sender, PacketReader reader)
{{
	{readMessageDataCode}
	
	Handle_{method.Name}(sender{paramNamesCode});
	on{method.Name}?.Invoke(sender{paramNamesCode});
}}";

				return code;
			})
			.Join($"{Environment.NewLine}{Environment.NewLine}");

			return handleMessageCode;
		}

		string GenerateAbstractMessageMethodCode(string senderTypeName, IEnumerable<MethodInfo> methods)
		{
			var code = methods.Select(method =>
			{
				var paramTypeNamesCode = CodeGenerationUtility.GenerateParameterTypeNameCode(method.GetParameters());

				return $@"protected abstract void Handle_{method.Name}({senderTypeName} sender{paramTypeNamesCode});";
			})
			.JoinWithLine();

			return code;
		}
	}
}
