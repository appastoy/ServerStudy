//-------------------------------------------------------
//
// Auto Generated Code. Do NOT edit this.
//
//-------------------------------------------------------

using Neti.Packets;

namespace Neti.Echo
{
	public static partial class ServerToClient
	{
		public abstract class MessageHandling
		{
			public delegate void ResponseEchoHandler(TcpClient sender, string message);

			ResponseEchoHandler onResponseEcho;

			public event ResponseEchoHandler OnResponseEcho { add { onResponseEcho += value; } remove { onResponseEcho -= value; } }

			public void Handle(TcpClient sender, PacketReader reader)
			{
				try
				{
					reader.Reset();

					var messageId = reader.Read<ushort>();
					switch (messageId)
					{
						case MessageId.ResponseEcho: HandleResponseEcho(sender, reader); break;

						default: throw new UnknownMessageException(messageId);
					}
				}
				finally
				{
					reader.Use();
				}
			}

			void HandleResponseEcho(TcpClient sender, PacketReader reader)
			{
				var message = reader.ReadString();

				Handle_ResponseEcho(sender, message);
				onResponseEcho?.Invoke(sender, message);
			}

			protected abstract void Handle_ResponseEcho(TcpClient sender, string message);
		}
	}
}