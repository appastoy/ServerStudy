using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Neti.Pool
{
	class GenericPool<T>
	{
		readonly ConcurrentStack<T> freeStack = new ConcurrentStack<T>();

		volatile int allocCount = 0;
		
		public int AllocCount => allocCount;
		public int FreeCount => freeStack.Count;

		public T Alloc()
		{
			Interlocked.Increment(ref allocCount);
			return freeStack.TryPop(out var item) ? item : OnAllocate();
		}

		public void Free(T item)
		{
			Interlocked.Decrement(ref allocCount);
			freeStack.Push(item);
		}

		protected virtual T OnAllocate()
		{
			return Activator.CreateInstance<T>();
		}

		protected virtual void OnFree(T _)
		{

		}
	}
}
