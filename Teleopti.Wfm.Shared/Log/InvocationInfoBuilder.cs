using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class InvocationInfoBuilder
	{
		public string BuildInvocationStart(IInvocationInfo invocation)
		{
			return $"{invocation.TargetType}.{invocation.Method.Name}({string.Join(", ", getParametersAndArguments(invocation))})";
		}

		public string BuildInvocationEnd(Exception exception, IInvocationInfo invocation, TimeSpan? elapsed)
		{
			var exceptionOccured = "";
			var elapsedTime = "";
			var result = "";

			if (exception != null)
				exceptionOccured = " (exception occured during execution)";
			if (elapsed.HasValue)
				elapsedTime = $" ({elapsed.Value:c})";
			if (invocation.Method.ReturnType != typeof(void))
				result = $" resulted with {formatValue(invocation.ReturnValue)}";

			return $"{invocation.TargetType}./{invocation.Method.Name}{result}{elapsedTime}{exceptionOccured}";
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
			if (!(argument is string) && argument is IEnumerable collection)
				return "Count = " + collection.Cast<object>().Count();
			return argument;
		}
	}
}