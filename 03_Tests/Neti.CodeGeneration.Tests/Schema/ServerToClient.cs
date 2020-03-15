using Neti.Schema;

namespace Neti.CodeGeneration.Tests.Schema
{
	[MessageGroupToClient(200)]
	public interface ServerToClient
	{
		void Response1(uint uintParam);
		void Response2(bool boolParam, decimal decimalParam);
	}
}
