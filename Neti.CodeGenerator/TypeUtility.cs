using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neti.Scheme;

namespace Neti.CodeGenerator
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

		static readonly string sessionTypeName = "TcpSession";
		static readonly string clientTypeName = "TcpClient";

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

		public static string GetRpcClientTypeName(MessageGroupAttribute messageGroup)
		{
			switch (messageGroup)
			{
				case MessageGroupToServerAttribute _: return clientTypeName;
				case MessageGroupToClientAttribute _: return sessionTypeName;
				default: throw new ArgumentException(nameof(messageGroup));
			}
		}

		public static string GetHandlerClientTypeName(MessageGroupAttribute messageGroup)
		{
			switch (messageGroup)
			{
				case MessageGroupToServerAttribute _: return clientTypeName;
				case MessageGroupToClientAttribute _: return sessionTypeName;
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
	}
}
