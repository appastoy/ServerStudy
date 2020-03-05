using Neti.Packets;

namespace Neti.Echo
{
	public static partial class ServerToClient
	{
		public abstract class MessageHandling
		{
			public void Handle(TcpClient sender, PacketReader reader)
			{
				try
				{
					reader.Reset();

					var protocolId = reader.Read<ushort>();
					switch (protocolId)
					{
						case MessageId.ResponseEcho: HandleResponseEcho(sender, in reader); break;

						default: throw new UnknownMessageException(protocolId);
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
