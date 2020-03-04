using System;

namespace Neti.Protocols
{
	[Serializable]
	public sealed class UnknownProtocolException : Exception
	{
		public readonly ushort ProtocolId;

		public UnknownProtocolException(ushort protocolId) : base($"Unknown protocol id. ({protocolId})")
		{
			ProtocolId = protocolId;
		}
	}
}