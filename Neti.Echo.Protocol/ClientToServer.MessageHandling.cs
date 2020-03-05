using Neti.Packets;

namespace Neti.Echo
{
	public static partial class ClientToServer
	{
		public abstract class MessageHandling
		{
			public void Handle(TcpSession session, PacketReader reader)
			{
				try
				{
					reader.Reset();

					var protocolId = reader.Read<ushort>();
					switch (protocolId)
					{
						case MessageId.RequestEcho: HandleRequestEcho(session, in reader); break;

						default: throw new UnknownMessageException(protocolId);
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
