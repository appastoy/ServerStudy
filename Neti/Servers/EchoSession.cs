using Neti.Buffer;

namespace Neti.Servers
{
	class EchoSession : TcpClient
	{
		protected override void OnBytesReceived(IStreamBufferReader reader)
		{
			var clonedBuffer = BufferUtility.CloneBuffer(reader.Buffer, reader.ReadPosition, reader.ReadableSize);
			reader.ExternalRead(reader.ReadableSize);
			SendAsync(clonedBuffer);
		}
	}
}
