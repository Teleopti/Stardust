using System;
using System.Diagnostics;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class TestLogTimeAspect : IAspect
	{
		private readonly Stopwatch _stopwatch;
		private readonly TestLog _log;

		public TestLogTimeAspect(TestLog log)
		{
			_log = log;
			_stopwatch = new Stopwatch();
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_log.Debug($"{invocation.Method.DeclaringType}.{invocation.Method.Name}");
			_stopwatch.Start();
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			_stopwatch.Stop();
			var message = $@"{invocation.Method.DeclaringType}./{invocation.Method.Name}: {_stopwatch.Elapsed:hh\:mm\:ss}";
			if (exception != null)
				message += " (exception occured during execution)";
			_log.Debug(message);
		}
	}
}