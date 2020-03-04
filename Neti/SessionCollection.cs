using System;
using System.Collections;
using System.Collections.Generic;

namespace Neti
{
	public class SessionCollection : IReadOnlySessionCollection, IEnumerable<TcpSession>
	{
		readonly List<TcpSession> sessions;
		readonly Dictionary<int, TcpSession> sessionMap;

		public int Count
		{
			get
			{
				lock (this)
				{
					return sessions.Count;
				}
			}
		}

		public SessionCollection()
		{
			sessions = new List<TcpSession>();
			sessionMap = new Dictionary<int, TcpSession>();
		}

		public SessionCollection(int capacity)
		{
			sessions = new List<TcpSession>(capacity);
			sessionMap = new Dictionary<int, TcpSession>(capacity);
		}

		public void Add(TcpSession session)
		{
			if (session is null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			lock (this)
			{
				sessions.Add(session);
				sessionMap.Add(session.Id, session);
			}
		}

		public TcpSession Find(int id)
		{
			lock (this)
			{
				return sessionMap.TryGetValue(id, out var session) ? session : null;
			}
		}

		public bool Contains(int id)
		{
			lock (this)
			{
				return sessionMap.ContainsKey(id);
			}
		}

		public void Remove(int id)
		{
			lock (this)
			{
				if (sessionMap.TryGetValue(id, out var session))
				{
					sessions.Remove(session);
					sessionMap.Remove(id);
				}
			}
		}

		public void Clear()
		{
			lock (this)
			{
				sessions.Clear();
				sessionMap.Clear();
			}
		}

		public List<TcpSession>.Enumerator GetEnumerator()
		{
			return sessions.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return sessions.GetEnumerator();
		}

		IEnumerator<TcpSession> IEnumerable<TcpSession>.GetEnumerator()
		{
			return sessions.GetEnumerator();
		}
	}
}
