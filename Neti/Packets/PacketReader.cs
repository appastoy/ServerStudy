using System;
using System.Text;
using Neti.Buffer;

namespace Neti.Packets
{
	public struct PacketReader
	{
		readonly StreamBuffer _streamBuffer;
		int _readPosition;

		public byte[] Buffer => _streamBuffer.Buffer;
		public int Offset { get; }
		public int Count { get; }
		public bool IsUsed { get; private set; }

		public int ReadPosition => Offset + _readPosition + 2;
		public int PacketSize => Count - 2;


		public PacketReader(StreamBuffer streamBuffer, int count)
		{
			_streamBuffer = streamBuffer ?? throw new ArgumentNullException(nameof(streamBuffer));
			Offset = _streamBuffer.ProcessingPosition;
			Count = count;
			_streamBuffer.ExternalProcess(count);
			_readPosition = 0;
			IsUsed = false;
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

		public void Use()
		{
			if (IsUsed)
			{
				return;
			}

			IsUsed = true;
			_streamBuffer.ExternalRead(Count);
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
