using Neti.Schema;

namespace Neti.Echo
{
	[MessageGroupToServer(100)]
	public interface ClientToServer
	{
		void RequestEcho(string message);
	}
}
