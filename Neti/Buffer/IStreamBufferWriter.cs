namespace Neti.Buffer
{
	public interface IStreamBufferWriter : IStreamBuffer
	{
        int WritePosition { get; set; }
        int WritableSize { get; }

        void Write<T>(in T value) where T : unmanaged;

        void Write(byte[] bytes);

        void Write(byte[] bytes, int offset, int count);

        void ExternalWrite(int byteSize);
    }
}
