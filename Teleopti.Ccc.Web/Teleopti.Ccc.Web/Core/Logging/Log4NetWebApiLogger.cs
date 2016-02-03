using System;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;

namespace Teleopti.Ccc.Web.Core.Logging
{
	[CLSCompliant(false)]
	public class Log4NetWebApiLoggerFilter : ExceptionFilterAttribute
	{
		private readonly Log4NetLogger _log4NetLogger;

		public Log4NetWebApiLoggerFilter(Log4NetLogger log4NetLogger)
		{
			_log4NetLogger = log4NetLogger;
		}

		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			base.OnException(actionExecutedContext);
			_log4NetLogger.LogException(actionExecutedContext.Exception);
		}
	}

    [CLSCompliant(false)]
    public class Log4NetWebApiLogger : ExceptionLogger
    {
        //  #36971 Inheriting from ExceptionLogger will not log for cancelled requests
        private readonly Log4NetLogger _log4NetLogger;

        public Log4NetWebApiLogger(Log4NetLogger log4NetLogger)
        {
            _log4NetLogger = log4NetLogger;
        }

        public override void Log(ExceptionLoggerContext context)
        {
            base.Log(context);
            _log4NetLogger.LogException(context.Exception);
        }
    }
}