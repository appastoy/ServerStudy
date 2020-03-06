using Neti.Scheme;

namespace Neti.Echo
{
	[MessageGroupToClient(200)]
	public interface ServerToClient
	{
		void ResponseEcho(string message);
	}
}
