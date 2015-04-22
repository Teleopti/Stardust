using System;
using log4net;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class InfoLogAttribute : ResolvedAspectAttribute
	{
		public InfoLogAttribute()
			: base(typeof(ILogAspect))
		{
		}
	}


	public interface ILogAspect : IAspect
	{
	}

	public class LogAspect : ILogAspect
	{
		private readonly ILogManagerWrapper _logManagerWrapper;

		public LogAspect(ILogManagerWrapper logManagerWrapper)
		{
			_logManagerWrapper = logManagerWrapper;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var logger = _logManagerWrapper.GetLogger(invocation.InvocationTarget.GetType());
			logger.Info(invocation.Method);
		}

		public void OnAfterInvocation(Exception exception)
		{
		}
	}



	public interface ILogManagerWrapper
	{
		ILog GetLogger(Type type);
	}

	public class LogManagerWrapper : ILogManagerWrapper
	{
		public ILog GetLogger(Type type)
		{
			return LogManager.GetLogger(type);
		}
	}
}