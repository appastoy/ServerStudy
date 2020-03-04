using System;

namespace Neti.Buffer
{
    partial class StreamBuffer
	{
        int writePosition;

        public int WritePosition
        {
            get => writePosition + Offset;
            set => writePosition = value - Offset;
        }

        public int WritableSize => Capacity - WritePosition;

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

            WriteBytes(bytes, offset, count);
        }

        public void ExternalWrite(int byteSize)
        {
            CheckWritable(byteSize);

            WritePosition += byteSize;
        }

        void CheckWritable(int byteSize)
        {
            if (byteSize < 0 ||
                byteSize > WritableSize)
            {
                throw new InvalidOperationException("Invalid byteSize on write.");
            }
        }

        unsafe void WriteValue<T>(in T value) where T : unmanaged
        {
            var size = sizeof(T);
            CheckWritable(size);

            fixed (T* source = &value)
            fixed (byte* destination = &Buffer[WritePosition])
            {
                WritePosition += size;
                System.Buffer.MemoryCopy(source, destination, WritableSize, size);
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
