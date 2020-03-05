using Neti.Scheme;

namespace Neti.Echo
{
	[MeesageGroupToClient(200)]
	public interface ServerToClient
	{
		void ResponseEcho(string message);
	}
}
