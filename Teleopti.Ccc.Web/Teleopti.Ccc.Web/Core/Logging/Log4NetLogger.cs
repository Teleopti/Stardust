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
			else if (httpException != null && (httpException.ErrorCode == unchecked((int) 0x80070057) ||
					 httpException.ErrorCode == unchecked((int) 0x800703E3)))
			{
				//  0x80070057 ignore “The remote host closed the connection” exception from signalr, https://github.com/SignalR/SignalR/issues/2632
				//  0x800703E3 ignore “The remote host closed the connection” exception, https://stackoverflow.com/questions/8286037/the-remote-host-closed-the-connection-error-how-fix
			}
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