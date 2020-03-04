using System;

namespace Neti.Protocols
{
	public readonly struct ProtocolId : IEquatable<ProtocolId>
	{
		public readonly ushort Value;

		public ProtocolId(ushort value)
		{
			Value = value;
		}

		public bool Equals(ProtocolId other)
		{
			return Value == other.Value;
		}

		public static explicit operator ushort (ProtocolId id)
		{
			return id.Value;
		}

		public static explicit operator ProtocolId (ushort value)
		{
			return new ProtocolId(value);
		}
	}
}
