using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Neti.Pool
{
	public class GenericPool<T>
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

		public (T Item, bool IsCreated) AllocEx()
		{
			Interlocked.Increment(ref allocCount);
			if (freeStack.TryPop(out var item))
			{
				return (item, false);
			}
			else
			{
				return (OnAllocate(), true);
			}
		}

		public void Free(T item)
		{
			Interlocked.Decrement(ref allocCount);
			freeStack.Push(item);
		}

		public void Clear()
		{
			freeStack.Clear();
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
