using System;
using System.Collections.Generic;
using System.Text;

namespace Neti
{
    public interface IBuffer
    {
        byte[] Buffer { get; }
        int Capacity { get; }
    }

    public interface IReadableBuffer
    {
        int WrittenSize { get; }
        int ReadableSize { get; }
        int ReadPosition { get; }

        ushort ReadUInt16();
        void ExternalRead(int byteSize);
        void ResetForRead();
    }

    public interface IWritableBuffer
    {
        int WrittenSize { get; }
        int WritableSize { get; }
        int WritePosition { get; }

        void Write(ushort value);
        void Write(byte[] bytes);
        void Write(byte[] bytes, int offset, int count);
        void ExternalWrite(int byteSize);
        void ResetForWrite();
    }

    class StreamBuffer : IReadableBuffer, IWritableBuffer
    {
        public byte[] Buffer { get; }
        public int WritePosition { get; private set; }
        public int ReadPosition { get; private set; }
        public int Capacity => Buffer.Length;
        public int WritableSize => Capacity - WritePosition;
        public int ReadableSize => WritePosition - ReadPosition;
        public int WrittenSize => Buffer[ReadPosition] | (Buffer[ReadPosition + 1] << 8);

        public StreamBuffer() : this(4096)
        {

        }

        public StreamBuffer(int size)
        {
            if (size < 1024)
            {
                size = 1024;
            }

            Buffer = new byte[size];
        }

        public void Write(ushort value)
        {
            CheckWritable(2);

            Buffer[WritePosition    ] = (byte)(value & 0xff);
            Buffer[WritePosition + 1] = (byte)((value >> 8) & 0xff);
            OnWrite(2);
        } 

        public void Write(byte[] bytes)
        {
            Write(bytes, 0, bytes != null ? bytes.Length : 0);
        }

        public void Write(byte[] bytes, int offset, int count)
        {
            WriteBytes(bytes, offset, count);
        }

        public ushort ReadUInt16()
        {
            CheckReadable(2);

            var value = Buffer[ReadPosition] |
                        (Buffer[ReadPosition + 1] << 8);
            ReadPosition += 2;

            return (ushort)value;
        }

        public void ExternalWrite(int byteSize)
        {
            CheckWritable(byteSize);
            WritePosition += byteSize;
        }

        public void ExternalRead(int byteSize)
        {
            CheckReadable(byteSize);
            ReadPosition += byteSize;

            if (ReadPosition == WritePosition)
            {
                ResetForRead();
            }
        }

        public void ResetForRead()
        {
            ReadPosition = WritePosition = 0;
        }

        public void ResetForWrite()
        {
            ReadPosition = 0;
            WritePosition = 2;
            SetWrittenSize(0);
        }

        void CheckWritable(int byteSize)
        {
            if (byteSize > WritableSize)
            {
                throw new InvalidOperationException($"Can't write. (Query: {byteSize}, Free: {WritableSize})");
            }
        }

        void CheckReadable(int byteSize)
        {
            if (byteSize > ReadableSize)
            {
                throw new InvalidOperationException($"Can't read. (Query: {byteSize}, Written: {ReadableSize})");
            }
        }

        unsafe void WriteBytes(byte[] bytes, int offset, int count)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (count < 0 ||
                count > bytes.Length)
            {
                throw new ArgumentException($"Invalid count({count}).");
            }

            if (offset < 0 ||
               (offset + count) > bytes.Length)
            {
                throw new ArgumentException($"Invalid offset({offset}).");
            }

            CheckWritable(count);

            fixed (byte* source = &bytes[offset])
            fixed (byte* destination = &Buffer[WritePosition])
            {
                System.Buffer.MemoryCopy(source, destination, WritableSize, count);
            }
            OnWrite(count);
        }

        void OnWrite(int byteSize)
        {
            WritePosition += byteSize;
            SetWrittenSize(ReadableSize);
        }

        void SetWrittenSize(int byteSize)
        {
            Buffer[ReadPosition    ] = (byte)(byteSize & 0xff);
            Buffer[ReadPosition + 1] = (byte)((byteSize >> 8) & 0xff);
        }
    }
}
