using Neti.Schema;

namespace Neti.Echo
{
	[MessageGroupToClient(200)]
	public interface ServerToClient
	{
		void ResponseEcho(string message);
	}
}
