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
			string clientTypeName = TypeUtility.GetRpcClientTypeName(messageGroup);

			return GenerateRpcCode(type, type.GetMethods(), clientTypeName);
		}

		string GenerateRpcCode(Type messageGroupType, IEnumerable<MethodInfo> rpcMethods, string clientTypeName)
		{
			var rpcMethodCodes = rpcMethods.Select(method => GenerateRpcMethodCode(method, clientTypeName));
			var rpcCode = string.Join($"{Environment.NewLine}{Environment.NewLine}", rpcMethodCodes);
			var indentedRpcCode = rpcCode.Replace(Environment.NewLine, $"{Environment.NewLine}{CodeConstants.MessageCodeIndent}");

			var paramTypes = rpcMethods.SelectMany(method => method.GetParameters().Select(param => param.ParameterType))
									   .Distinct();
			var usingCode = TypeUtility.GetUnfriendlyTypeUsingCode(paramTypes, messageGroupType.Namespace);

			return CodeUtility.BuildMessageGroupCode(usingCode,
													 messageGroupType.Namespace,
													 messageGroupType.Name,
													"Rpc",
													indentedRpcCode);
		}

		string GenerateRpcMethodCode(MethodInfo method, string clientTypeName)
		{
			var parameters = method.GetParameters();
			var parameterCode = GenerateParameterCode(parameters);
			var writeCodes = parameters.Select(param => $"writer.Write({param.Name});");
			var writeCode = string.Join($"{Environment.NewLine}		", writeCodes);
	
			return
$@"public void {method.Name}({clientTypeName} sender, {parameterCode})
{{
	using (var writer = sender.CreatePacketWriter())
	{{
		writer.Write(MessageId.{method.Name});
		{writeCode}
	}}
}}";
		}

		string GenerateParameterCode(ParameterInfo[] parameters)
		{
			var paramCodes = parameters.Select(param => $"{TypeUtility.GetFriendlyTypeName(param.ParameterType)} {param.Name}");
			
			return string.Join(", ", paramCodes);
		}
	}
}
