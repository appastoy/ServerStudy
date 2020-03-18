//-------------------------------------------------------
//
// Auto Generated Code. Do NOT edit this.
//
//-------------------------------------------------------

// No using

namespace Neti.Echo
{
	public static partial class ServerToClient
	{
		public static class Rpc
		{
			public static void ResponseEcho(TcpSession sender, string message)
			{
				using (var writer = sender.CreatePacketWriter())
				{
					writer.Write(MessageId.ResponseEcho);
					writer.Write(message);
				}
			}
		}
	}
}