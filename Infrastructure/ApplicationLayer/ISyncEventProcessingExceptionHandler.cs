using System;
using log4net;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface ISyncEventProcessingExceptionHandler
	{
		void Handle(Exception e);
	}

	public class LogExceptions : ISyncEventProcessingExceptionHandler
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(SyncEventPublisher));

		public void Handle(Exception e)
		{
			_logger.Error("Sync event processing failed", e);
		}
	}

	public class ThrowExceptions : ISyncEventProcessingExceptionHandler
	{
		public void Handle(Exception e)
		{
			throw e;
		}
	}
}