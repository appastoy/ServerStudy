using System;

namespace Neti.Buffer
{
    public partial class StreamBuffer : IStreamBufferReader, IStreamBufferWriter
    {
        public const int DefaultSize = 4096;

        public byte[] Buffer { get; }
        public int Offset { get; }
        public int Capacity { get; }

        public int ProcessedSize { get; private set; }
        public int ProcessableSize => ReadableSize - ProcessedSize;
        public int ProcessingPosition => ReadPosition + ProcessedSize;

        public StreamBuffer() : this(DefaultSize)
        {

        }

        public StreamBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException($"capacity is smaller than 1.", nameof(capacity));
            }

            Buffer = new byte[capacity];
            Offset = 0;
            Capacity = capacity;
        }

        public StreamBuffer(byte[] buffer) : this(buffer, 0, buffer != null ? buffer.Length : 0)
        {

        }

        public StreamBuffer(byte[] buffer, int offset, int count)
        {
            Validator.ValidateBytes(buffer, offset, count);

            if (count <= 0)
            {
                throw new ArgumentException($"count is smaller than 1.", nameof(count));
            }

            Buffer = buffer;
            Offset = 0;
            Capacity = count;
        }

        public void ResetPosition()
        {
            WritePosition = ReadPosition = ProcessedSize = 0;
        }

        public void ExternalProcess()
        {
            ProcessedSize += ProcessableSize;
        }
    }
}
