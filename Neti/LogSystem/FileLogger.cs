using System;
using System.IO;
using System.Text;

namespace Neti.LogSystem
{
	public sealed class FileLogger : ILogger, IDisposable
	{
		StreamWriter streamWriter;
		public string OutputPath { get; }
		public bool WriteTime { get; }

		public FileLogger(string outputPath, bool writeTime = true)
		{
			OutputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
			WriteTime = writeTime;
			streamWriter = new StreamWriter(outputPath, true, Encoding.UTF8);
			streamWriter.AutoFlush = true;
		}

		public void Clear()
		{
			
		}

		public void LogError(string message)
		{
			Log($"E {message}");	
		}

		public void LogInfo(string message)
		{
			Log($"I {message}");
		}

		public void LogWarning(string message)
		{
			Log($"W {message}");
		}

		void Log(string message)
		{
			var finalMessage = WriteTime ? $"{DateTime.Now:HH:mm:ss} {message}" : message;
			streamWriter.WriteLine(finalMessage);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~FileLogger()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			streamWriter?.Dispose();
			streamWriter = null;
		}
	}
}
