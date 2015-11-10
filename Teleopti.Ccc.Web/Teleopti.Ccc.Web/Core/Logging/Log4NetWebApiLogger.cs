using System;
using System.Web.Http.Filters;

namespace Teleopti.Ccc.Web.Core.Logging
{
	[CLSCompliant(false)]
	public class Log4NetWebApiLogger : ExceptionFilterAttribute
	{
		private readonly Log4NetLogger _log4NetLogger;

		public Log4NetWebApiLogger(Log4NetLogger log4NetLogger)
		{
			_log4NetLogger = log4NetLogger;
		}

		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			base.OnException(actionExecutedContext);
			_log4NetLogger.LogException(actionExecutedContext.Exception);
		}
	}
}