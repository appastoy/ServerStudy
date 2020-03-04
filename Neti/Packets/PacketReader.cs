using System;
using System.Text;
using Neti.Buffer;

namespace Neti.Packets
{
	public struct PacketReader
	{
		readonly StreamBuffer streamBuffer;
		int readPosition;

		public byte[] Buffer => streamBuffer.Buffer;
		public int Offset { get; }
		public int Count { get; }
		public bool IsUsed { get; private set; }

		public int ReadPosition => Offset + readPosition + 2;
		public int PacketSize => Count - 2;


		public PacketReader(StreamBuffer streamBuffer, int count)
		{
			this.streamBuffer = streamBuffer ?? throw new ArgumentNullException(nameof(streamBuffer));
			Offset = this.streamBuffer.ProcessingPosition;
			Count = count;
			this.streamBuffer.ExternalProcess(count);
			readPosition = 0;
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
			readPosition += size;

			return bytesSegment;
		}

		public string ReadString()
		{
			var bytesSegment = ReadBytes();
			if (bytesSegment.Count > 0)
			{
				return Encoding.UTF8.GetString(bytesSegment.Array, bytesSegment.Offset, bytesSegment.Count);
			}
			else
			{
				return string.Empty;
			}
		}

		public void Use()
		{
			if (IsUsed)
			{
				return;
			}

			IsUsed = true;
			streamBuffer.ExternalRead(Count);
		}

		unsafe T ReadValue<T>() where T : unmanaged
		{
			var size = sizeof(T);
			CheckReadable(size);

			fixed (byte* pointer = &Buffer[ReadPosition])
			{
				readPosition += size;
				return *(T*)pointer;
			}
		}

		void CheckReadable(int size)
		{
			if (readPosition + size > PacketSize)
			{
				throw new InvalidOperationException("Can't read.");
			}
		}

		public void Reset()
		{
			readPosition = 0;
		}
	}
}
