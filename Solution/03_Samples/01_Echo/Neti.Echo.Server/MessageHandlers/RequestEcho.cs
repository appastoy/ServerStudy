using Neti.LogSystem;

namespace Neti.Echo.Server.MessageHandlers
{
	partial class ClientToServerMessageHandler
	{
		protected override void Handle_RequestEcho(TcpSession session, string message)
		{
			Logger.LogInfo($"{session.RemoteAddress}:{session.RemotePort} > {message}");
			ServerToClient.Rpc.ResponseEcho(session, message);
			session.FlushPacketsAsync();
		}
	}
}
