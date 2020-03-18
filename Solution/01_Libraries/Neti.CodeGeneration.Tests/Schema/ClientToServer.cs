using Neti.Schema;

namespace Neti.CodeGeneration.Tests.Schema
{
	[MessageGroupToServer(100)]
	public interface IClientToServer
	{
		void Request1(int intParam);
		void Request2(string stringParam, float floatParam);
	}
}
