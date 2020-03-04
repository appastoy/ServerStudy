using Neti.LogSystem;

namespace Neti.Echo.Server.MessageHandlers
{
	partial class ClientToServerMessageHandler
	{
		protected override void RequestEcho(TcpSession session, string message)
		{
			Logger.LogInfo($"{session.RemoteAddress}:{session.RemotePort} > {message}");
			Rpc.ServerToClient.ResponseEcho(session, message);
			session.FlushPacketsAsync();
		}
	}
}
