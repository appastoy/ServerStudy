﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Neti.LogSystem
{
	public enum LogType
	{
		Info,
		Warning,
		Error
	}

	readonly struct LogData
	{
		public readonly LogType Type;
		public readonly string Message;

		public LogData(LogType type, string message)
		{
			Type = type;
			Message = message ?? throw new ArgumentNullException(nameof(message));
		}
	}

	public static class Logger
	{
		static int flushRateMilliSeconds;

		static readonly List<ILogger> loggers = new List<ILogger>();
		static readonly ConcurrentQueue<LogData> logDatas = new ConcurrentQueue<LogData>();

		public static bool AutoFlush { get; private set; }
		public static int FlushRateMilliSeconds
		{
			get => flushRateMilliSeconds;
			set => flushRateMilliSeconds = Math.Max(0, value);
		}

		public static void AddLogger(ILogger logger)
		{
			if (logger is null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			lock (loggers)
			{
				loggers.Add(logger);
			}
		}

		public static TLogger FindLogger<TLogger>() where TLogger : ILogger
		{
			return loggers.OfType<TLogger>().FirstOrDefault();
		}

		public static TLogger[] FindLoggers<TLogger>() where TLogger : ILogger
		{
			return loggers.OfType<TLogger>().ToArray();
		}

		public static void RemoveLogger(ILogger logger)
		{
			if (logger is null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			lock (loggers)
			{
				loggers.Remove(logger);
			}
		}

		public static void ClearLoggers()
		{
			lock (loggers)
			{
				loggers.Clear();
			}
		}

		public static void LogInfo(string message)
		{
			logDatas.Enqueue(new LogData(LogType.Info, message));
		}

		public static void LogWarning(string message)
		{
			logDatas.Enqueue(new LogData(LogType.Warning, message));
		}

		public static void LogError(string message)
		{
			logDatas.Enqueue(new LogData(LogType.Error, message));
		}

		public static void Log(LogType type, string message)
		{
			switch (type)
			{
				case LogType.Info:	LogInfo(message); break;
				case LogType.Warning: LogWarning(message); break;
				case LogType.Error: LogError(message); break;
			}
		}

		public static void Flush()
		{
			lock (loggers)
			{
				while (logDatas.TryDequeue(out var logData))
				{
					switch (logData.Type)
					{
						case LogType.Info:
							foreach (var logger in loggers)
							{
								logger.LogInfo(logData.Message);
							}
							break;

						case LogType.Warning:
							foreach (var logger in loggers)
							{
								logger.LogWarning(logData.Message);
							}
							break;

						case LogType.Error:
							foreach (var logger in loggers)
							{
								logger.LogError(logData.Message);
							}
							break;
					}
				}
			}
		}
		
		public static void Clear()
		{
			lock (loggers)
			{
				foreach (var logger in loggers)
				{
					logger.Clear();
				}
			}
		}

		public static void EnableAutoFlush()
		{
			if (AutoFlush)
			{
				return;
			}

			AutoFlush = true;
			Task.Run(UpdateFlush);
		}

		public static void DisableAutoFlush()
		{
			AutoFlush = false;
		}

		static void UpdateFlush()
		{
			while (AutoFlush)
			{
				Flush();
				if (flushRateMilliSeconds <= 0)
				{
					Thread.Yield();
				}
				else
				{
					Thread.Sleep(flushRateMilliSeconds);
				}
			}
		}
	}
}