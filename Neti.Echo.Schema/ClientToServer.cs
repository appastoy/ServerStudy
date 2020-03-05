using Neti.Scheme;

namespace Neti.Echo
{
	[MessageGroupToServer(100)]
	public interface ClientToServer
	{
		void RequestEcho(string message);
	}
}
