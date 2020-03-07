using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Neti.CodeGenerator.Generators
{
	public class RpcCodeGenerator : ICodeGenerator
	{
		public string Generate(Type type)
		{
			var messageGroup = TypeUtility.GetMessageGroupAttribute(type);
			string senderTypeName = TypeUtility.GetRpcSenderTypeName(messageGroup);

			return GenerateRpcCode(type, type.GetMethods(), senderTypeName);
		}

		string GenerateRpcCode(Type messageGroupType, IEnumerable<MethodInfo> rpcMethods, string senderTypeName)
		{
			var usingCode = TypeUtility.GetUnfriendlyParameterTypeUsingCode(rpcMethods, messageGroupType.Namespace);
			var rpcCode = rpcMethods.Select(method => GenerateRpcMethodCode(method, senderTypeName))
									.Join($"{Environment.NewLine}{Environment.NewLine}")
									.InsertAfterEachLine(CodeConstants.InternalClassCodeIndent);

			return CodeUtility.BuildMessageGroupCode(usingCode,
													 messageGroupType.Namespace,
													 messageGroupType.Name,
													 "Rpc",
													 rpcCode);
		}

		string GenerateRpcMethodCode(MethodInfo method, string senderTypeName)
		{
			var parameters = method.GetParameters();
			var parameterCode = CodeUtility.GenerateParameterTypeNameCode(parameters);
			var writeCode = parameters.Select(param => $"{Environment.NewLine}		writer.Write({param.Name});").Join();
	
			return
$@"public void {method.Name}({senderTypeName} sender{parameterCode})
{{
	using (var writer = sender.CreatePacketWriter())
	{{
		writer.Write(MessageId.{method.Name});{writeCode}
	}}
}}";
		}
	}
}
