using System;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
	public class RetryingEventArgs : EventArgs
	{
		public RetryingEventArgs(int currentRetryCount, TimeSpan delay, Exception lastException)
		{
			this.CurrentRetryCount = currentRetryCount;
			this.Delay = delay;
			this.LastException = lastException;
		}

		public int CurrentRetryCount { get; private set; }

		public TimeSpan Delay { get; private set; }

		public Exception LastException { get; private set; }
	}
}