using System;
using System.Text;
using Neti.Buffer;

namespace Neti.Packets
{
	public struct PacketWriter : IDisposable
	{
		readonly StreamBuffer _streamBuffer;

		int _writableSize;

		public byte[] Buffer => _streamBuffer.Buffer;
		public int Offset { get; }
		public int Count => PacketSize + 2;
		
		public int PacketSize { get; private set; }

		public PacketWriter(StreamBuffer buffer)
		{
			_streamBuffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

			Offset = _streamBuffer.WritePosition;
			PacketSize = 0;
			_streamBuffer.Write<short>(0);
			_writableSize = Math.Min(_streamBuffer.WritableSize, ushort.MaxValue);
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

			CheckWritable(count);
			_streamBuffer.Write(bytes, offset, count);
			_writableSize -= count;
		}

		public void Write(string value)
		{
			if (value is null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			Write(Encoding.UTF8.GetBytes(value));
		}

		unsafe void WriteValue<T>(in T value) where T : unmanaged
		{
			var size = sizeof(T);
			CheckWritable(size);

			_streamBuffer.Write(in value);
			_writableSize -= size;
		}

		void CheckWritable(int size)
		{
			if (size > _writableSize)
			{
				throw new InvalidOperationException("Can't write.");
			}
		}

		public void Reset()
		{
			_streamBuffer.WritePosition = Offset + 2;
			_writableSize = Math.Min(_streamBuffer.WritableSize, ushort.MaxValue);
		}

		public void Submit()
		{
			var currentWritePosition = _streamBuffer.WritePosition;
			PacketSize = currentWritePosition - Offset - 2;
			_streamBuffer.WritePosition = Offset;
			_streamBuffer.Write((ushort)PacketSize);
			_streamBuffer.WritePosition = currentWritePosition;
		}

		public void Dispose()
		{
			Submit();
		}
	}
}
