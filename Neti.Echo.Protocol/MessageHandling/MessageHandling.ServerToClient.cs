using Neti.Packets;
using Neti.Protocols;

namespace Neti.Echo
{
	public static partial class MessageHandling
	{
		public abstract class ServerToClient
		{
			public void Handle(TcpClient sender, PacketReader reader)
			{
				try
				{
					reader.Reset();

					var protocolId = reader.Read<ushort>();
					switch (protocolId)
					{
						case Definition.ServerToClient.MessageId.ResponseEcho: HandleResponseEcho(sender, in reader); break;

						default: throw new UnknownProtocolException(protocolId);
					}
				}
				finally
				{
					reader.Use();
				}
			}

			protected abstract void ResponseEcho(TcpClient sender, string message);

			void HandleResponseEcho(TcpClient sender, in PacketReader reader)
			{
				var message = reader.ReadString();

				ResponseEcho(sender, message);
			}
		}
	}
}
