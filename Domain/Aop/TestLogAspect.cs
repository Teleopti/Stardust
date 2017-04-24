using System;
using System.Diagnostics;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class TestLogAspect : IAspect
	{
		private readonly Stopwatch _stopwatch;
		private readonly TestLog _log;
		private readonly InvocationInfoBuilder _builder;

		public TestLogAspect(TestLog log)
		{
			_log = log;
			_stopwatch = new Stopwatch();
			_builder = new InvocationInfoBuilder();
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			if (!_log.IsEnabled())
				return;
			_log.Debug(_builder.BuildInvocationStart(invocation));
			_stopwatch.Start();
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			if (!_log.IsEnabled())
				return;
			_stopwatch.Stop();
			_log.Debug(_builder.BuildInvocationEnd(exception, invocation, _stopwatch.Elapsed));
		}
	}
}