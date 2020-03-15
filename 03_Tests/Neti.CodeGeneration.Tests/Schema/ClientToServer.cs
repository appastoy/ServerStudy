using Neti.Schema;

namespace Neti.CodeGeneration.Tests.Schema
{
	[MessageGroupToServer(100)]
	public interface ClientToServer
	{
		void Request1(int intParam);
		void Request2(string stringParam, float floatParam);
	}
}
