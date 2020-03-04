using Neti.LogSystem;

namespace Neti.Echo.Client.MessageHandlers
{
	partial class ServerToClientMessageHandler
	{
		protected override void ResponseEcho(TcpClient sender, string message)
		{
			Logger.LogInfo($"{sender.RemoteAddress}:{sender.RemotePort} > {message}");
		}
	}
}
