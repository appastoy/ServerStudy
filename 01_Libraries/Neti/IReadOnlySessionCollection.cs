using System.Collections.Generic;

namespace Neti
{
	public interface IReadOnlySessionCollection : IEnumerable<TcpSession>
	{
		int Count { get; }

		TcpSession Find(int id);

		bool Contains(int id);

		new List<TcpSession>.Enumerator GetEnumerator();
	}
}
