using System;
using System.Text;

namespace Neti.Packets
{
	public struct PacketReader
	{
		int _readPosition;

		public byte[] Buffer { get; }
		public int Offset { get; }
		public int Count { get; }

		public int ReadPosition => Offset + _readPosition + 2;
		public int PacketSize => Count - 2;


		public PacketReader(byte[] bytes, int offset, int count)
		{
			Buffer = bytes ?? throw new ArgumentNullException(nameof(bytes));
			Offset = offset;
			Count = count;
			_readPosition = 0;
		}

		public T Read<T>() where T : unmanaged
		{
			return ReadValue<T>();
		}

		public ArraySegment<byte> ReadBytes()
		{
			var size = ReadValue<ushort>();
			CheckReadable(size);

			var bytesSegment = new ArraySegment<byte>(Buffer, ReadPosition, size);
			_readPosition += size;

			return bytesSegment;
		}

		public string ReadString()
		{
			var bytesSegment = ReadBytes();
			return Encoding.UTF8.GetString(bytesSegment.Array, bytesSegment.Offset, bytesSegment.Count);
		}

		unsafe T ReadValue<T>() where T : unmanaged
		{
			var size = sizeof(T);
			CheckReadable(size);

			fixed (byte* pointer = &Buffer[ReadPosition])
			{
				_readPosition += size;

				return *(T*)pointer;
			}
		}

		void CheckReadable(int size)
		{
			if (_readPosition + size > PacketSize)
			{
				throw new InvalidOperationException("Can't read.");
			}
		}

		public void Reset()
		{
			_readPosition = 0;
		}
	}
}
