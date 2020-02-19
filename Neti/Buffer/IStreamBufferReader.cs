namespace Neti.Buffer
{
	public interface IStreamBufferReader : IStreamBuffer
	{
		int ReadableSize { get; } 
		int ReadPosition { get; }

		T Read<T>() where T : unmanaged;
		T Peek<T>() where T : unmanaged;
		void ExternalRead(int byteSize);
	}
}
