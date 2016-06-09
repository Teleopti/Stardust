using System;
using System.Web;
using System.Web.Configuration;
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

			var innerMostException = getInnerMostException(exception);

			if (innerMostException is HttpException && ((HttpException)innerMostException).GetHttpCode() == 404)
				_logger.Warn(Log4NetModule.LogMessage404, innerMostException);
			else
				_logger.Error(Log4NetModule.LogMessageException, innerMostException);
		}

		private static Exception getInnerMostException(Exception exception)
		{
			while (exception.InnerException != null)
				exception = exception.InnerException;
			return exception;
		}
	}
}