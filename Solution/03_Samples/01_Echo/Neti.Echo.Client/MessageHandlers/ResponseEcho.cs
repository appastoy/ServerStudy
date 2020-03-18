using Neti.LogSystem;

namespace Neti.Echo.Client.MessageHandlers
{
	partial class ServerToClientMessageHandler
	{
		protected override void Handle_ResponseEcho(TcpClient sender, string message)
		{
			Logger.LogInfo($"{sender.RemoteAddress}:{sender.RemotePort} > {message}");
		}
	}
}
