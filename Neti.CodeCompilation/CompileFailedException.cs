using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Neti.CodeCompilation
{
	public sealed class CompileFailedException : Exception
	{
		public IReadOnlyList<Diagnostic> Errors { get; }

		public CompileFailedException(IReadOnlyList<Diagnostic> errors) : base(
			string.Join(Environment.NewLine,
						new string[] { $"Compile failed. (Error: {errors.Count})" }
							.Concat(errors.Select(diag => diag.ToString()))))
		{
			Errors = errors;
		}
	}
}
