using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neti.Echo
{
	public static class Waiting
	{
		public static void Until(Func<bool> func)
		{
			Task.Run(() => { while (func.Invoke() == false) Thread.Yield(); }).Wait();
		}
	}
}
