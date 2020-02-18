using System;

namespace Neti.Buffer
{
    public class StreamBuffer : IStreamBufferReader
    {
        public const int DefaultSize = 4096;
        public const int MinimumSize = 1024;

        public byte[] Buffer { get; }
        public int Capacity => Buffer.Length;
        public int WritePosition { get; set; }
        public int ReadPosition { get; set; }
        public int WritableSize => Capacity - WritePosition;
        public int ReadableSize => WritePosition - ReadPosition;

        public StreamBuffer() : this(DefaultSize)
        {

        }

        public StreamBuffer(int capacity)
        {
            if (capacity < MinimumSize)
            {
                capacity = MinimumSize;
            }

            Buffer = new byte[capacity];
            ResetPosition();
        }

        public void ResetPosition()
        {
            WritePosition = ReadPosition = 0;
        }

        public ushort ReadUInt16()
        {
            CheckReadable(2);

            var value = (ushort)(Buffer[ReadPosition++] |
                                (Buffer[ReadPosition++] << 8));
            ResetPositionIfNotReadable();

            return value;
        }
        
        public ushort PeekUInt16()
        {
            CheckReadable(2);

            return (ushort)(Buffer[ReadPosition    ] |
                           (Buffer[ReadPosition + 1] << 8));
        }

        public void ExternalRead(int byteSize)
        {
            CheckReadable(byteSize);

            ReadPosition += byteSize;

            ResetPositionIfNotReadable();
        }

        public void Write(ushort value)
        {
            CheckWritable(2);

            Buffer[WritePosition++] = (byte)(value & 0xff);
            Buffer[WritePosition++] = (byte)((value >> 8) & 0xff);
        }

        public void Write(byte[] bytes)
        {
            Write(bytes, 0, bytes != null ? bytes.Length : 0);
        }

        public void Write(byte[] bytes, int offset, int count)
        {
            WriteBytes(bytes, offset, count);
        }

        public void ExternalWrite(int byteSize)
        {
            CheckWritable(byteSize);

            WritePosition += byteSize;
        }

        void CheckReadable(int byteSize)
        {
            if (byteSize < 0 ||
                byteSize > ReadableSize)
            {
                throw new InvalidOperationException("Invalid byteSize on read.");
            }
        }

        void CheckWritable(int byteSize)
        {
            if (byteSize < 0 ||
                byteSize > WritableSize)
            {
                throw new InvalidOperationException("Invalid byteSize on write.");
            }
        }

        void ResetPositionIfNotReadable()
        {
            if (ReadableSize <= 0)
            {
                WritePosition = ReadPosition = 0;
            }
        }

        unsafe void WriteBytes(byte[] bytes, int offset, int count)
        {
            Validator.ValidateBytes(bytes, offset, count);

            fixed (byte* source = &bytes[offset])
            fixed (byte* destination = &Buffer[WritePosition])
            {
                System.Buffer.MemoryCopy(source, destination, WritableSize, count);
            }

            WritePosition += count;
        }
    }
}
