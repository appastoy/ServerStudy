using FluentAssertions;
using Neti.CodeGenerator.Generators;
using Neti.Scheme;
using NUnit.Framework;

namespace Neti.Mock
{
	[MessageGroupToServer(100)]
	interface MockMessageGroup
	{
		void MockMethod1(bool p4, char p5, string p6);
		void MockMethod3(sbyte p7, short p8, int p9, long p10);
		void MockMethod2(byte p11, ushort p12, uint p13, ulong p14);
		void MockMethod0(float p15, double p16, decimal p17);
	}
}

namespace Neti.CodeGenerator.Tests
{
	public class CodeGeneratorTest
	{
		[Test]
		public void MessageIdCodeGenerateTest()
		{
			string expectedMessageIdCode =
$@"{CodeConstants.AutoGeneratedHeader}

// No using

namespace Neti.Mock
{{
	public static partial class MockMessageGroup
	{{
		public static class MessageId
		{{
			public const ushort MockMethod1 = 100;
			public const ushort MockMethod3 = 101;
			public const ushort MockMethod2 = 102;
			public const ushort MockMethod0 = 103;
		}}
	}}
}}";

			var generator = new MessageIdCodeGenerator();
			generator.Generate(typeof(Mock.MockMessageGroup)).Should().Be(expectedMessageIdCode);
			generator.GenerateAsync(typeof(Mock.MockMessageGroup)).Result.Should().Be(expectedMessageIdCode);
		}

		[Test]
		public void RpcCodeGenerateTest()
		{
			string expectedRpcCode =
$@"{CodeConstants.AutoGeneratedHeader}

// No using

namespace Neti.Mock
{{
	public static partial class MockMessageGroup
	{{
		public static class Rpc
		{{
			public void MockMethod1(TcpClient sender, bool p4, char p5, string p6)
			{{
				using (var writer = sender.CreatePacketWriter())
				{{
					writer.Write(MessageId.MockMethod1);
					writer.Write(p4);
					writer.Write(p5);
					writer.Write(p6);
				}}
			}}

			public void MockMethod3(TcpClient sender, sbyte p7, short p8, int p9, long p10)
			{{
				using (var writer = sender.CreatePacketWriter())
				{{
					writer.Write(MessageId.MockMethod3);
					writer.Write(p7);
					writer.Write(p8);
					writer.Write(p9);
					writer.Write(p10);
				}}
			}}

			public void MockMethod2(TcpClient sender, byte p11, ushort p12, uint p13, ulong p14)
			{{
				using (var writer = sender.CreatePacketWriter())
				{{
					writer.Write(MessageId.MockMethod2);
					writer.Write(p11);
					writer.Write(p12);
					writer.Write(p13);
					writer.Write(p14);
				}}
			}}

			public void MockMethod0(TcpClient sender, float p15, double p16, decimal p17)
			{{
				using (var writer = sender.CreatePacketWriter())
				{{
					writer.Write(MessageId.MockMethod0);
					writer.Write(p15);
					writer.Write(p16);
					writer.Write(p17);
				}}
			}}
		}}
	}}
}}";

			var generator = new RpcCodeGenerator();
			generator.Generate(typeof(Mock.MockMessageGroup)).Should().Be(expectedRpcCode);
			generator.GenerateAsync(typeof(Mock.MockMessageGroup)).Result.Should().Be(expectedRpcCode);
		}

		[Test]
		public void MessageHandlingCodeGenerateTest()
		{
			string expectedMessageHandlingCode =
$@"{CodeConstants.AutoGeneratedHeader}

// No using

namespace Neti.Mock
{{
	public static partial class MockMessageGroup
	{{
		public static class MessageHandling
		{{
			public delegate void MockMethod1Handler(TcpSession sender, bool p4, char p5, string p6);
			public delegate void MockMethod3Handler(TcpSession sender, sbyte p7, short p8, int p9, long p10);
			public delegate void MockMethod2Handler(TcpSession sender, byte p11, ushort p12, uint p13, ulong p14);
			public delegate void MockMethod0Handler(TcpSession sender, float p15, double p16, decimal p17);

			MockMethod1Handler onMockMethod1;
			MockMethod3Handler onMockMethod3;
			MockMethod2Handler onMockMethod2;
			MockMethod0Handler onMockMethod0;

			public event MockMethod1Handler OnMockMethod1 {{ add {{ onMockMethod1 += value; }} remove {{ onMockMethod1 -= value; }} }}
			public event MockMethod3Handler OnMockMethod3 {{ add {{ onMockMethod3 += value; }} remove {{ onMockMethod3 -= value; }} }}
			public event MockMethod2Handler OnMockMethod2 {{ add {{ onMockMethod2 += value; }} remove {{ onMockMethod2 -= value; }} }}
			public event MockMethod0Handler OnMockMethod0 {{ add {{ onMockMethod0 += value; }} remove {{ onMockMethod0 -= value; }} }}

			public void Handle(TcpSession sender, PacketReader reader)
			{{
				try
				{{
					reader.Reset();

					var messageId = reader.Read<ushort>();
					switch (messageId)
					{{
						case MessageId.MockMethod1: HandleMockMethod1(sender, reader); break;
						case MessageId.MockMethod3: HandleMockMethod3(sender, reader); break;
						case MessageId.MockMethod2: HandleMockMethod2(sender, reader); break;
						case MessageId.MockMethod0: HandleMockMethod0(sender, reader); break;

						default: throw new UnknownMessageException(messageId);
					}}
				}}
				finally
				{{
					reader.Use();
				}}
			}}

			void HandleMockMethod1(TcpSession sender, PacketReader reader)
			{{
				var p4 = reader.Read<bool>();
				var p5 = reader.Read<char>();
				var p6 = reader.ReadString();

				Handle_MockMethod1(sender, p4, p5, p6);
				onMockMethod1?.Invoke(sender, p4, p5, p6);
			}}

			void HandleMockMethod3(TcpSession sender, PacketReader reader)
			{{
				var p7 = reader.Read<sbyte>();
				var p8 = reader.Read<short>();
				var p9 = reader.Read<int>();
				var p10 = reader.Read<long>();

				Handle_MockMethod3(sender, p7, p8, p9, p10);
				onMockMethod3?.Invoke(sender, p7, p8, p9, p10);
			}}

			void HandleMockMethod2(TcpSession sender, PacketReader reader)
			{{
				var p11 = reader.Read<byte>();
				var p12 = reader.Read<ushort>();
				var p13 = reader.Read<uint>();
				var p14 = reader.Read<ulong>();

				Handle_MockMethod2(sender, p11, p12, p13, p14);
				onMockMethod2?.Invoke(sender, p11, p12, p13, p14);
			}}

			void HandleMockMethod0(TcpSession sender, PacketReader reader)
			{{
				var p15 = reader.Read<float>();
				var p16 = reader.Read<double>();
				var p17 = reader.Read<decimal>();

				Handle_MockMethod0(sender, p15, p16, p17);
				onMockMethod0?.Invoke(sender, p15, p16, p17);
			}}

			protected abstract void Handle_MockMethod1(TcpSession sender, bool p4, char p5, string p6);
			protected abstract void Handle_MockMethod3(TcpSession sender, sbyte p7, short p8, int p9, long p10);
			protected abstract void Handle_MockMethod2(TcpSession sender, byte p11, ushort p12, uint p13, ulong p14);
			protected abstract void Handle_MockMethod0(TcpSession sender, float p15, double p16, decimal p17);
		}}
	}}
}}";

			var generator = new MessageHandlingCodeGenerator();
			generator.Generate(typeof(Mock.MockMessageGroup)).Should().Be(expectedMessageHandlingCode);
			generator.GenerateAsync(typeof(Mock.MockMessageGroup)).Result.Should().Be(expectedMessageHandlingCode);
		}
	}
}