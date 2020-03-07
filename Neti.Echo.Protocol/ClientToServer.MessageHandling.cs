using Neti.Packets;

namespace Neti.Echo
{
	public static partial class ClientToServer
	{
		public abstract class MessageHandling
		{
			public delegate void RequestEchoHandler(TcpSession sender, string message);

			RequestEchoHandler onRequestEcho;

			public event RequestEchoHandler OnRequestEcho { add { onRequestEcho += value; } remove { onRequestEcho -= value; } }

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

			void HandleRequestEcho(TcpSession sender, in PacketReader reader)
			{
				var message = reader.ReadString();

				RequestEcho(sender, message);
				onRequestEcho?.Invoke(sender, message);
			}

			protected abstract void RequestEcho(TcpSession session, string message);
		}
	}
}
