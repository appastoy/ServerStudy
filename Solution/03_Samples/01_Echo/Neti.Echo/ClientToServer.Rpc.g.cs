//-------------------------------------------------------
//
// Auto Generated Code. Do NOT edit this.
//
//-------------------------------------------------------

// No using

namespace Neti.Echo
{
	public static partial class ClientToServer
	{
		public static class Rpc
		{
			public static void RequestEcho(TcpClient sender, string message)
			{
				using (var writer = sender.CreatePacketWriter())
				{
					writer.Write(MessageId.RequestEcho);
					writer.Write(message);
				}
			}
		}
	}
}