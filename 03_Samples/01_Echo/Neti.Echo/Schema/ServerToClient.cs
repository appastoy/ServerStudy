using Neti.Schema;

namespace Neti.Echo
{
	[MessageGroupToClient(200)]
	public interface IServerToClient
	{
		void ResponseEcho(string message);
	}
}
