using System;

namespace Neti.Buffer
{
    partial class StreamBuffer : IStreamBufferReader
	{
        int _readPosition;

		public int ReadPosition
        {
            get => _readPosition + Offset;
            set => _readPosition = value - Offset;
        }

		public int ReadableSize => WritePosition - ReadPosition;

        public T Read<T>() where T : unmanaged
        {
            return ReadValue<T>();
        }

        public T Peek<T>() where T : unmanaged
        {
            return PeekValue<T>();
        }

        public void ExternalRead(int byteSize)
        {
            CheckReadable(byteSize);

            ReadPosition += byteSize;
            ResetPositionIfNotReadable();
        }

        unsafe T ReadValue<T>() where T : unmanaged
        {
            var size = sizeof(T);
            CheckReadable(size);

            fixed (byte* pointer = &Buffer[ReadPosition])
            {
                ReadPosition += size;
                ResetPositionIfNotReadable();

                return *(T*)pointer;
            }
        }

        unsafe T PeekValue<T>() where T : unmanaged
        {
            CheckReadable(sizeof(T));

            fixed (byte* pointer = &Buffer[ReadPosition])
            {
                return *(T*)pointer;
            }
        }

        void CheckReadable(int byteSize)
        {
            if (byteSize < 0 ||
                byteSize > ReadableSize)
            {
                throw new InvalidOperationException("Invalid byteSize on read.");
            }
        }

        void ResetPositionIfNotReadable()
        {
            if (ReadableSize <= 0)
            {
                ResetPosition();
            }
        }
    }
}
