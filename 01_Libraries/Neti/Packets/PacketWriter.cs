using System;
using System.Text;
using Neti.Buffer;

namespace Neti.Packets
{
	public struct PacketWriter : IDisposable
	{
		readonly StreamBuffer streamBuffer;

		int writableSize;

		public byte[] Buffer => streamBuffer.Buffer;
		public int Offset { get; }
		public int Count => PacketSize + 2;
		
		public int PacketSize { get; private set; }

		public PacketWriter(StreamBuffer buffer)
		{
			streamBuffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

			Offset = streamBuffer.WritePosition;
			PacketSize = 0;
			streamBuffer.Write<short>(0);
			writableSize = Math.Min(streamBuffer.WritableSize, ushort.MaxValue);
		}

		public void Write<T>(in T value) where T : unmanaged
		{
			WriteValue(in value);
		}

		public void Write(byte[] bytes)
		{
			Write(bytes, 0, bytes != null ? bytes.Length : 0);
		}

		public void Write(byte[] bytes, int offset, int count)
		{
			if (bytes is null)
			{
				throw new ArgumentNullException(nameof(bytes));
			}

			WriteValue((ushort)count);

			if (count > 0)
			{
				CheckWritable(count);
				streamBuffer.Write(bytes, offset, count);
				writableSize -= count;
			}
		}

		public void Write(string value)
		{
			if (value is null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (value.Length > 0)
			{
				Write(Encoding.UTF8.GetBytes(value));
			}
			else
			{
				Write<ushort>(0);
			}
		}

		unsafe void WriteValue<T>(in T value) where T : unmanaged
		{
			var size = sizeof(T);
			CheckWritable(size);

			streamBuffer.Write(in value);
			writableSize -= size;
		}

		void CheckWritable(int size)
		{
			if (size > writableSize)
			{
				throw new InvalidOperationException("Can't write.");
			}
		}

		public void Reset()
		{
			streamBuffer.WritePosition = Offset + 2;
			writableSize = Math.Min(streamBuffer.WritableSize, ushort.MaxValue);
		}

		public void Submit()
		{
			var currentWritePosition = streamBuffer.WritePosition;
			PacketSize = currentWritePosition - Offset - 2;
			streamBuffer.WritePosition = Offset;
			streamBuffer.Write((ushort)PacketSize);
			streamBuffer.WritePosition = currentWritePosition;
		}

		public void Dispose()
		{
			Submit();
		}
	}
}
