using System.Net.Sockets;

namespace Neti.Pool
{
	class SendAsyncEventArgsPool : GenericPool<SocketAsyncEventArgs>
	{
		public static readonly SendAsyncEventArgsPool Instance = new SendAsyncEventArgsPool();

		protected override SocketAsyncEventArgs OnAllocate()
		{
			var eventArgs = new SocketAsyncEventArgs();
			eventArgs.Completed += (_, e) => Free(e);

			return eventArgs;
		}

		protected override void OnFree(SocketAsyncEventArgs e)
		{
			e.SetBuffer(null, 0, 0);
		}
	}
}
