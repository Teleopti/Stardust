using System;
using System.Web;
using log4net;

namespace Teleopti.Ccc.Web.Core.Logging
{
	public class Log4NetLogger
	{
		private readonly ILog _logger;

		public Log4NetLogger(ILog logger)
		{
			_logger = logger;
		}

		public void LogException(Exception exception)
		{
			var httpException = getInnerMostException(exception) as HttpException;
			if (httpException != null && httpException.GetHttpCode() == 404)
				_logger.Warn(Log4NetModule.LogMessage404, exception);
			else
				_logger.Error(Log4NetModule.LogMessageException, exception);
		}

		private static Exception getInnerMostException(Exception exception)
		{
			while (exception.InnerException != null)
				exception = exception.InnerException;
			return exception;
		}
	}
}