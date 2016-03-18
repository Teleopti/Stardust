using System.Linq;
using Castle.DynamicProxy;
using log4net;

namespace Stardust.Node.Log4Net.Interceptors
{
	public class Log4NetInterceptor : IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
			var logger =
				LogManager.GetLogger(invocation.TargetType);

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Calling method {0} with parameters {1}... ",
				                   invocation.Method.Name,
				                   string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray()));
			}

			invocation.Proceed();

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Done: result was {0}.", invocation.ReturnValue);
			}
		}
	}
}