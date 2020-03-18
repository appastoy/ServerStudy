using Neti.Schema;

namespace Neti.Echo
{
	[MessageGroupToServer(100)]
	public interface IClientToServer
	{
		void RequestEcho(string message);
	}
}
