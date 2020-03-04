namespace Neti.Echo
{
	public static partial class Rpc
	{
		public static class ServerToClient
		{
			public static void ResponseEcho(TcpClient sender, string message)
			{
				using (var writer = sender.CreatePacketWriter())
				{
					writer.Write(Definition.ServerToClient.MessageId.ResponseEcho);
					writer.Write(message);
				}
			}
		}
	}
}
