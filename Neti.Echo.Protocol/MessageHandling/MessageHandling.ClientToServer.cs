using Neti.Packets;
using Neti.Protocols;

namespace Neti.Echo
{
	public static partial class MessageHandling
	{
		public abstract class ClientToServer
		{
			public void Handle(TcpSession session, PacketReader reader)
			{
				try
				{
					reader.Reset();

					var protocolId = reader.Read<ushort>();
					switch (protocolId)
					{
						case Definition.ClientToServer.MessageId.RequestEcho: HandleRequestEcho(session, in reader); break;

						default: throw new UnknownProtocolException(protocolId);
					}
				}
				finally
				{
					reader.Use();
				}
			}

			protected abstract void RequestEcho(TcpSession session, string message);

			void HandleRequestEcho(TcpSession sender, in PacketReader reader)
			{
				var message = reader.ReadString();

				RequestEcho(sender, message);
			}
		}
	}
}
