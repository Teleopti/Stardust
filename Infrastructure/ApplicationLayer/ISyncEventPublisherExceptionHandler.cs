using System;
using System.Runtime.ExceptionServices;
using log4net;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface ISyncEventPublisherExceptionHandler
	{
		void Handle(Exception e);
	}

	public class LogExceptions : ISyncEventPublisherExceptionHandler
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(SyncEventPublisher));

		public void Handle(Exception e)
		{
			_logger.Error("Sync event publishing failed", e);
		}
	}

	public class ThrowExceptions : ISyncEventPublisherExceptionHandler
	{
		public void Handle(Exception e)
		{
			throw e;
		}
	}
}