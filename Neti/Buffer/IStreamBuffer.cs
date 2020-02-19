namespace Neti.Buffer
{
	public interface IStreamBuffer
	{
		byte[] Buffer { get; }
		int Capacity { get; }
	}
}
