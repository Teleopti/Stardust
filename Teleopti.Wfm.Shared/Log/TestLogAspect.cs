using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class TestLogAspect : IAspect
	{
		private readonly TestLog _log;
		private readonly InvocationInfoBuilder _builder;

		private readonly ThreadLocal<Stack<Stopwatch>> _stopwatch = new ThreadLocal<Stack<Stopwatch>>(() => new Stack<Stopwatch>());

		public TestLogAspect(TestLog log)
		{
			_log = log;
			_builder = new InvocationInfoBuilder();
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			if (!_log.IsEnabled())
				return;
			_log.Debug(_builder.BuildInvocationStart(invocation));
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			_stopwatch.Value.Push(stopwatch);
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			if (!_log.IsEnabled())
				return;
			var stopwatch = _stopwatch.Value.Pop();
			stopwatch.Stop();
			_log.Debug(_builder.BuildInvocationEnd(exception, invocation, stopwatch.Elapsed));
		}
	}
}