using System;

namespace Neti
{
	[Serializable]
	public sealed class UnknownMessageException : Exception
	{
		public readonly ushort ProtocolId;

		public UnknownMessageException(ushort protocolId) : base($"Unknown protocol id. ({protocolId})")
		{
			ProtocolId = protocolId;
		}
	}
}