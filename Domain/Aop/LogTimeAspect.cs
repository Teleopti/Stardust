using System;
using System.Diagnostics;
using log4net;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class LogTimeAspect : IAspect
	{
		private readonly Stopwatch _stopwatch;
		private readonly ILog _logger;

		public LogTimeAspect(ILogManagerWrapper logger)
		{
			_logger = logger.GetLogger("Teleopti.LogTime");
			_stopwatch = new Stopwatch();
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_logger.Debug($"{invocation.Method.DeclaringType}.{invocation.Method.Name}");
			_stopwatch.Start();
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_stopwatch.Stop();
			var message = $@"{invocation.Method.DeclaringType}./{invocation.Method.Name}: {_stopwatch.Elapsed:hh\:mm\:ss}";
			if (exception != null)
				message += " (exception occured during execution)";
			_logger.Debug(message);
		}
	}
}