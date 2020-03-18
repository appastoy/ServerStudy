using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Neti.CodeGeneration.Generators
{
	public class RpcCodeGenerator : ICodeGenerator
	{
		const string localPathPostfix = ".Rpc.g.cs";

		public CodeGenerationResult Generate(Type type)
		{
			var messageGroup = TypeUtility.GetMessageGroupAttribute(type);
			string senderTypeName = TypeUtility.GetRpcSenderTypeName(messageGroup);

			var localPath = $"{type.Name.TrimStart('I')}{localPathPostfix}";
			var code = GenerateRpcCode(type, type.GetMethods(), senderTypeName);

			return new CodeGenerationResult(localPath, code);
		}

		string GenerateRpcCode(Type messageGroupType, IEnumerable<MethodInfo> rpcMethods, string senderTypeName)
		{
			var usingCode = TypeUtility.GetUnfriendlyParameterTypeUsingCode(rpcMethods, messageGroupType.Namespace);
			var rpcCode = rpcMethods.Select(method => GenerateRpcMethodCode(method, senderTypeName))
									.Join($"{Environment.NewLine}{Environment.NewLine}")
									.InsertAfterEachLine(CodeConstants.InternalClassCodeIndent);

			return CodeGenerationUtility.BuildMessageGroupCode(usingCode,
															   messageGroupType.Namespace,
															   messageGroupType.Name.TrimStart('I'),
															   "static class Rpc",
															   rpcCode);
		}

		string GenerateRpcMethodCode(MethodInfo method, string senderTypeName)
		{
			var parameters = method.GetParameters();
			var parameterCode = CodeGenerationUtility.GenerateParameterTypeNameCode(parameters);
			var writeCode = parameters.Select(param => $"{Environment.NewLine}		writer.Write({param.Name});").Join();
	
			return
$@"public static void {method.Name}({senderTypeName} sender{parameterCode})
{{
	using (var writer = sender.CreatePacketWriter())
	{{
		writer.Write(MessageId.{method.Name});{writeCode}
	}}
}}";
		}
	}
}
