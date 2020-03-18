using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace Neti.CodeCompilation
{
	public struct CompilationResult
	{
		string message;
		string fullLog;

		public bool Success { get; }
		public Assembly Assembly { get; }
		
		public IReadOnlyList<Diagnostic> Warning { get; }
		public IReadOnlyList<Diagnostic> Error { get; }
		
		public string Message
		{
			get
			{
				if (string.IsNullOrEmpty(message))
				{
					message = BuildMessage();
				}

				return message;
			}
		}
		public string FullLog
		{
			get
			{
				if (string.IsNullOrEmpty(fullLog))
				{
					fullLog = BuildFullLog();
				}

				return fullLog;
			}
		}

		public CompilationResult(EmitResult emitResult, Assembly assembly)
		{
			if (emitResult == null)
			{
				throw new ArgumentNullException(nameof(emitResult));
			}

			Success = emitResult.Success;
			if(Success)
			{
				Assembly = assembly ?? throw new ArgumentNullException(nameof(emitResult));
			}
			else
			{
				Assembly = null;
			}

			Warning = emitResult.Diagnostics.Where(diag => diag.Severity == DiagnosticSeverity.Warning).ToArray();
			Error = emitResult.Diagnostics.Where(diag => diag.Severity == DiagnosticSeverity.Error).ToArray();
			message = null;
			fullLog = null;
		}

		string BuildMessage()
		{
			var diagnosticsMessage = new StringBuilder(128);

			var hasError = Error.Count > 0;
			var hasWarning = Warning.Count > 0;
			if (hasWarning || hasError)
			{
				diagnosticsMessage.Append(" (");
				if (hasError)
				{
					diagnosticsMessage.Append($"Error : {Error.Count}");
					if (hasWarning)
					{
						diagnosticsMessage.Append(", ");
					}
				}
				if (hasWarning)
				{
					diagnosticsMessage.Append($"Warning : {Warning.Count}");
				}
				diagnosticsMessage.Append(')');
			}

			return $"Compilation {(Success ? "succeeded" : "failed")}.{diagnosticsMessage}";
		}

		string BuildFullLog()
		{
			var fullLogBuilder = new StringBuilder(4096);
			if (Error.Count + Warning.Count > 0)
			{
				var logs = Error.Select(diag => diag.ToString())
								.Concat(Warning.Select(diag => diag.ToString()));
				foreach (var log in logs)
				{
					fullLogBuilder.AppendLine(log);
				}

				fullLogBuilder.AppendLine();
			}
			fullLogBuilder.AppendLine(Message);

			return fullLogBuilder.ToString();
		}

		public override string ToString()
		{
			return FullLog;
		}
	}
}
