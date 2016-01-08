using System;
using System.Diagnostics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Common.TimeLogger
{
	public class LogTimeAspect : IAspect
	{
		private const string output = "Time spent in method '{0}' on type '{1}': {2}";
		private const string loggerName = "Teleopti.LogTime";
		private readonly ILogManagerWrapper _logger;
		private readonly Stopwatch _stopwatch;

		public LogTimeAspect(ILogManagerWrapper logger)
		{
			_logger = logger;
			_stopwatch = new Stopwatch();
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_stopwatch.Start();
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_logger.GetLogger(loggerName)
				.DebugFormat(output, invocation.Method.Name, invocation.Method.DeclaringType, _stopwatch.Elapsed.ToString(@"hh\:mm\:ss"));
		}
	}
}