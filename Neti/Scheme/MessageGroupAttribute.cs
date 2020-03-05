using System;

namespace Neti.Scheme
{
	[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
	public abstract class MessageGroupAttribute : Attribute
	{
		public readonly ushort StartId;

		public MessageGroupAttribute(ushort startId)
		{
			StartId = startId;
		}
	}
}
