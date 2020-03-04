using System;

namespace Neti.Echo.Schema
{
	public interface ServerToClient
	{
		void ResponseEcho(string message);
	}
}
