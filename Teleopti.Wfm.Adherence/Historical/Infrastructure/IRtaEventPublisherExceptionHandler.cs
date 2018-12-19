using System;
using log4net;

namespace Teleopti.Wfm.Adherence.Historical.Infrastructure
{
	public interface IRtaEventPublisherExceptionHandler
	{
		void Handle(Exception e);
	}

	public class LogExceptionsFromRtaEventPublisher : IRtaEventPublisherExceptionHandler
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(LogExceptionsFromRtaEventPublisher));

		public void Handle(Exception e)
		{
			_logger.Error("Event publishing failed", e);
		}
	}

	public class ThrowExceptionsFromRtaEventPublisher : IRtaEventPublisherExceptionHandler
	{
		public void Handle(Exception e)
		{
			throw e;
		}
	}
}