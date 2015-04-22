using System;
using System.Text;
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
			var proxyType = invocation.TargetType;
			var logger = _logManagerWrapper.GetLogger(proxyType);
			if (!logger.IsInfoEnabled)
				return;
			var arguments = getArguments(invocation);

			logger.Info(proxyType + "." + invocation.Method.Name + arguments);
		}

		private static StringBuilder getArguments(IInvocationInfo invocation)
		{
			var arguments = new StringBuilder("(");
			var parameters = invocation.Method.GetParameters();
			for (var i = 0; i < parameters.Length; i++)
			{
				if (i > 0)
					arguments.Append(", ");
				var argument = invocation.Arguments[i];
				if (argument!= null && (argument as Array) != null)
					arguments.Append(parameters[i] + ": Count = " + ((Array) argument).Length);
				else
					arguments.Append(parameters[i] + ":" + argument);
			}
			arguments.Append(")");
			return arguments;
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