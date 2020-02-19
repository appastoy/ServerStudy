namespace Neti.Buffer
{
	public static class BufferUtility
	{
		public static unsafe byte[] CloneBuffer(byte[] buffer, int offset, int count)
		{
			Validator.ValidateBytes(buffer, offset, count);

			var newBuffer = new byte[count];
			fixed (byte* source = &buffer[offset])
			fixed (byte* destination = &newBuffer[0])
			{
				System.Buffer.MemoryCopy(source, destination, count, count);
			}

			return newBuffer;
		}
	}
}
