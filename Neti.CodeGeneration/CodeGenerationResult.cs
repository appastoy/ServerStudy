using System;

namespace Neti.CodeGeneration
{
	public readonly struct CodeGenerationResult
	{
		public readonly string LocalPath;
		public readonly string Code;

		public CodeGenerationResult(string localPath, string code)
		{
			LocalPath = localPath ?? throw new ArgumentNullException(nameof(localPath));
			Code = code ?? throw new ArgumentNullException(nameof(code));
		}
	}
}
