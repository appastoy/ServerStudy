using System;
using System.Collections.Generic;
using System.Text;

namespace Neti.Buffer
{
	public interface IStreamBufferReader
	{
		byte[] Buffer { get; }
		int Capacity { get; }
		int ReadableSize { get; } 
		int ReadPosition { get; }

		ushort ReadUInt16();
		ushort PeekUInt16();
		void ExternalRead(int byteSize);

	}
}
