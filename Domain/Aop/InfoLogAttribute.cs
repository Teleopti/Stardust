using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting.Export;

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
			var type = invocation.TargetType;
			var logger = _logManagerWrapper.GetLogger(type);
			if (!logger.IsInfoEnabled)
				return;
			logger.Info(type + "." + invocation.Method.Name + "(" + string.Join(", ", getParameterAndArgument(invocation)) + ")");
		}

		private static IEnumerable<string> getParameterAndArgument(IInvocationInfo invocation)
		{
			return 
				from @param in invocation.Method.GetParameters()
				let pos = @param.Position
				let argument = invocation.Arguments[pos]
				let argumentValue = formatObject(argument)
				select @param.Name + ": " + argumentValue;
		}

		private static object formatObject(object argument)
		{
			if (argument != null && (argument as Array) != null)
				return "Count = " + ((Array) argument).Length;
			return argument ?? "null";
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			var type = invocation.TargetType;
			var logger = _logManagerWrapper.GetLogger(type);
			if (!logger.IsInfoEnabled || invocation.Method.ReturnType == typeof(void))
				return;

			logger.Info(" - Result : " + formatObject(invocation.ReturnValue));
		}
	}
}