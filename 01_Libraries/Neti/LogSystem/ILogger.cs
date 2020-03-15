using System;
using System.Collections.Generic;
using System.Text;

namespace Neti.LogSystem
{
	public interface ILogger
	{
		void LogInfo(string message);
		void LogWarning(string message);
		void LogError(string message);

		void Clear();
	}
}
