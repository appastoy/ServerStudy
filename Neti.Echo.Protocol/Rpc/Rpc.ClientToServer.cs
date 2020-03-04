namespace Neti.Echo
{
	public static partial class Rpc
	{
		public static class ClientToServer
		{
			public static void RequestEcho(TcpClient sender, string message)
			{
				using (var writer = sender.CreatePacketWriter())
				{
					writer.Write(Definition.ClientToServer.MessageId.RequestEcho);
					writer.Write(message);
				}
			}
		}
	}
}
