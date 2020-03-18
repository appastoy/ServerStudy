//-------------------------------------------------------
//
// Auto Generated Code. Do NOT edit this.
//
//-------------------------------------------------------

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

			public void Handle(TcpSession sender, PacketReader reader)
			{
				try
				{
					reader.Reset();

					var messageId = reader.Read<ushort>();
					switch (messageId)
					{
						case MessageId.RequestEcho: HandleRequestEcho(sender, reader); break;

						default: throw new UnknownMessageException(messageId);
					}
				}
				finally
				{
					reader.Use();
				}
			}

			void HandleRequestEcho(TcpSession sender, PacketReader reader)
			{
				var message = reader.ReadString();

				Handle_RequestEcho(sender, message);
				onRequestEcho?.Invoke(sender, message);
			}

			protected abstract void Handle_RequestEcho(TcpSession sender, string message);
		}
	}
}