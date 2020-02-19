using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Neti.Pool
{
	class GenericPool<T>
	{
		readonly ConcurrentStack<T> _freeStack = new ConcurrentStack<T>();

		volatile int _allocCount = 0;
		
		public int AllocCount => _allocCount;
		public int FreeCount => _freeStack.Count;

		public T Alloc()
		{
			Interlocked.Increment(ref _allocCount);
			return _freeStack.TryPop(out var item) ? item : OnAllocate();
		}

		public void Free(T item)
		{
			Interlocked.Decrement(ref _allocCount);
			_freeStack.Push(item);
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
