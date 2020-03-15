using System;

namespace Neti.CodeCompilation
{
	public readonly struct CodeFile
	{
		public readonly string Path;
		public readonly string Code;

		public CodeFile(string path, string code)
		{
			Path = path ?? throw new ArgumentNullException(nameof(path));
			Code = code ?? throw new ArgumentNullException(nameof(code));
		}
	}
}
