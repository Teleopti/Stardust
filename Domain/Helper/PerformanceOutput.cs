using System;
using System.Diagnostics;
using log4net;

namespace Teleopti.Ccc.Domain.Helper
{
	/// <summary>
	/// Writes how long an operation takes
	/// within this scope.
	/// </summary>
	/// <example>
	/// using(PerformanceOutput.ForOperation("Loading persons"))
	/// {
	///     //do stuff here
	/// }
	/// </example>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-02-28
	/// </remarks>
	public sealed class PerformanceOutput : IDisposable
	{
		private static readonly ILog internalClassLogger = LogManager.GetLogger(typeof(PerformanceOutput));
		private readonly string _operation;
		private readonly Stopwatch _stopwatch;
		private readonly ILog _loggerToUse;

		private PerformanceOutput(string operation, Stopwatch stopwatch, ILog loggerToUse)
		{
			_operation = operation;
			_stopwatch = stopwatch;
			_loggerToUse = loggerToUse;
			_stopwatch.Start();
		}

		/// <summary>
		/// Logs <paramref name="operation"/> to this class logger.
		/// </summary>
		/// <param name="operation"><see cref="string"/> to log</param>
		/// <returns>An <see cref="IDisposable"/> object. Call dispose when you want the stop watch to stop.</returns>
		public static IDisposable ForOperation(string operation)
		{
			return ForOperation(operation, null);
		}

		/// <summary>
		/// Logs <paramref name="operation"/> to a logger called <paramref name="loggerName" />.
		/// </summary>
		/// <param name="operation"><see cref="string"/> to log</param>
		/// <param name="loggerName">Name of the logger</param>
		/// <returns>An <see cref="IDisposable"/> object. Call dispose when you want the stop watch to stop.</returns>
		/// <remarks>
		/// This is slower than calling <see cref="ForOperation(string)"/>. Do not use this overload in inner loops!
		/// </remarks>
		public static IDisposable ForOperation(string operation, string loggerName)
		{
			var logger = loggerToUse(loggerName);
			if (logger.IsInfoEnabled)
			{
				var retObj = new PerformanceOutput(operation, new Stopwatch(), logger);
				logger.Info(string.Concat(operation, " started."));
				return retObj;
			}
			return new disposeStub();
		}

		public void Dispose()
		{
			_stopwatch.Stop();
			_loggerToUse.Info(string.Concat(_operation, " took ", _stopwatch.Elapsed, " seconds."));
		}

		private static ILog loggerToUse(string loggerName)
		{
			return loggerName == null ? 
							internalClassLogger : 
							LogManager.GetLogger(loggerName);
		}

		private class disposeStub : IDisposable
		{
			//do nada impl
			public void Dispose()
			{
			}
		}
	}
}
