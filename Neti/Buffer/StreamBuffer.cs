namespace Neti.Buffer
{
    public partial class StreamBuffer : IStreamBufferReader, IStreamBufferWriter
    {
        public const int DefaultSize = 4096;
        public const int MinimumSize = 1024;

        public byte[] Buffer { get; }
        public int Capacity => Buffer.Length;

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
    }
}
