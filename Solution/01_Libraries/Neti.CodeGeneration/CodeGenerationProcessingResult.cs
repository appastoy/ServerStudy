using System;
using System.Collections.Generic;
using System.Text;

namespace Neti.CodeGeneration
{
	public struct CodeGenerationProcessingResult
	{
		string fullLog;

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

		public bool HasChanged => GeneratedFiles.Count + DeletedFiles.Count > 0;

		public readonly IReadOnlyList<string> GeneratedFiles;
		public readonly IReadOnlyList<string> DeletedFiles;

		public CodeGenerationProcessingResult(IReadOnlyList<string> updatedFiles, IReadOnlyList<string> deletedFiles)
		{
			GeneratedFiles = updatedFiles ?? throw new ArgumentNullException(nameof(updatedFiles));
			DeletedFiles = deletedFiles ?? throw new ArgumentNullException(nameof(deletedFiles));
			fullLog = null;
		}

		public override string ToString()
		{
			return FullLog;
		}

		string BuildFullLog()
		{
			var builder = new StringBuilder();
			if (HasChanged)
			{
				if (GeneratedFiles.Count > 0)
				{
					foreach (var file in GeneratedFiles)
					{
						builder.Append("GEN ");
						builder.AppendLine(file);
					}
				}

				if (DeletedFiles.Count > 0)
				{
					foreach (var file in DeletedFiles)
					{
						builder.Append("DEL ");
						builder.AppendLine(file);
					}
				}
				builder.AppendLine();
				builder.AppendLine($"Generation succeded. (Updated: {GeneratedFiles.Count}, Deleted: {DeletedFiles.Count})");
			}
			else
			{
				builder.AppendLine($"No changes.");
			}

			return builder.ToString();
		}
	}
}
