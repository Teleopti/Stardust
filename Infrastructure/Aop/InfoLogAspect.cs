using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public class InfoLogAspect : ILogAspect
	{
		private readonly ILogManagerWrapper _logManagerWrapper;

		public InfoLogAspect(ILogManagerWrapper logManagerWrapper)
		{
			_logManagerWrapper = logManagerWrapper;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var logger = _logManagerWrapper.GetLogger(invocation.TargetType.ToString());
			if (!logger.IsInfoEnabled)
				return;
			logger.Info(invocation.Method.Name + "(" + string.Join(", ", getParametersAndArguments(invocation)) + ")");
		}

		private static IEnumerable<string> getParametersAndArguments(IInvocationInfo invocation)
		{
			return 
				from @param in invocation.Method.GetParameters()
				let pos = @param.Position
				let argument = invocation.Arguments[pos]
				let argumentValue = formatValue(argument)
				select @param.Name + ": " + argumentValue;
		}

		private static object formatValue(object argument)
		{
			if (argument == null) 
				return "null";
			if (argument is Array)
				return "Count = " + ((Array) argument).Length;
			if (argument is IList)
				return "Count = " + ((IList) argument).Count;
			if (argument.GetType().IsGenericType)
				return "Enumerable";
			return argument;
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			var type = invocation.TargetType;
			var logger = _logManagerWrapper.GetLogger(type.ToString());
			if (!logger.IsInfoEnabled || invocation.Method.ReturnType == typeof(void))
				return;
			logger.Info("Result : " + formatValue(invocation.ReturnValue));
		}
	}
}