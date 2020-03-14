using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neti.Schema;

namespace Neti.CodeGeneration
{
	static class TypeUtility
	{
		static Dictionary<Type, string> friendlyTypeNameMap = new Dictionary<Type, string>
		{
			{ typeof(bool), "bool" },
			{ typeof(char), "char" },
			{ typeof(string), "string" },
			{ typeof(sbyte), "sbyte" },
			{ typeof(short), "short" },
			{ typeof(int), "int" },
			{ typeof(long), "long" },
			{ typeof(byte), "byte" },
			{ typeof(ushort), "ushort" },
			{ typeof(uint), "uint" },
			{ typeof(ulong), "ulong" },
			{ typeof(float), "float" },
			{ typeof(double), "double" },
			{ typeof(decimal), "decimal" },
		};

		const string sessionTypeName = "TcpSession";
		const string senderTypeName = "TcpClient";

		public static string GetFriendlyTypeName(Type type)
		{
			if (friendlyTypeNameMap.TryGetValue(type, out var name))
			{
				return name;
			}
			else
			{
				return type.Name;
			}
		}

		public static bool IsFriendlyType(Type type)
		{
			return friendlyTypeNameMap.ContainsKey(type);
		}

		public static string GetRpcSenderTypeName(MessageGroupAttribute messageGroup)
		{
			switch (messageGroup)
			{
				case MessageGroupToServerAttribute _: return senderTypeName;
				case MessageGroupToClientAttribute _: return sessionTypeName;
				default: throw new ArgumentException(nameof(messageGroup));
			}
		}

		public static string GetHandlerSenderTypeName(MessageGroupAttribute messageGroup)
		{
			switch (messageGroup)
			{
				case MessageGroupToServerAttribute _: return sessionTypeName;
				case MessageGroupToClientAttribute _: return senderTypeName;
				default: throw new ArgumentException(nameof(messageGroup));
			}
		}

		public static MessageGroupAttribute GetMessageGroupAttribute(Type type)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var attribute = type.GetCustomAttribute<MessageGroupAttribute>();
			if (attribute == null)
			{
				throw new ArgumentException(nameof(type));
			}

			return attribute;
		}

		public static string GetUnfriendlyParameterTypeUsingCode(IEnumerable<MethodInfo> methods, string exceptNamespace = null)
		{
			var paramTypes = methods.SelectMany(method => method.GetParameters().Select(param => param.ParameterType))
									.Distinct();

			return GetUnfriendlyTypeUsingCode(paramTypes, exceptNamespace);
		}

		public static string GetUnfriendlyTypeUsingCode(IEnumerable<Type> types, string execptNamespace = null)
		{
			if (types is null)
			{
				throw new ArgumentNullException(nameof(types));
			}

			var usingNamespaces = types.Where(type => IsFriendlyType(type) == false)
									   .Select(type => type.Namespace)
									   .Distinct()
									   .Where(namespaceName => namespaceName != execptNamespace)
									   .Select(namespaceName => $"using {namespaceName};");

			return string.Join(Environment.NewLine, usingNamespaces);
		}

		public static string GetPacketReaderReadMethodPostfix(Type parameterType)
		{
			if (parameterType == typeof(string))
			{
				return "String";
			}

			if (IsFriendlyType(parameterType))
			{
				return $"<{GetFriendlyTypeName(parameterType)}>";
			}

			throw new ArgumentException("not readable parameter type.", nameof(parameterType));
		}
	}
}
