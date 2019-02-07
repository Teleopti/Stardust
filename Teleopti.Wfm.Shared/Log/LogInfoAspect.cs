using System;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class LogInfoAspect : IAspect
	{
		private readonly ILogManager _logManager;
		private readonly InvocationInfoBuilder _builder;

		public LogInfoAspect(ILogManager logManager)
		{
			_logManager = logManager;
			_builder = new InvocationInfoBuilder();
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var log = _logManager.GetLogger(invocation.TargetType.ToString());
			if (!log.IsInfoEnabled)
				return;
			log.Info(_builder.BuildInvocationStart(invocation));
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			var log = _logManager.GetLogger(invocation.TargetType.ToString());
			if (!log.IsInfoEnabled)
				return;
			log.Info(_builder.BuildInvocationEnd(exception, invocation, null));
		}
	}
}