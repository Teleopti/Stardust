using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	public class ExponentialBackoff : RetryStrategy
	{
		private readonly int retryCount;
		private readonly TimeSpan minBackoff;
		private readonly TimeSpan maxBackoff;
		private readonly TimeSpan deltaBackoff;

		public ExponentialBackoff()
			: this(RetryStrategy.DefaultClientRetryCount, RetryStrategy.DefaultMinBackoff, RetryStrategy.DefaultMaxBackoff, RetryStrategy.DefaultClientBackoff)
		{
		}

		public ExponentialBackoff(int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
			: this((string)null, retryCount, minBackoff, maxBackoff, deltaBackoff, RetryStrategy.DefaultFirstFastRetry)
		{
		}

		public ExponentialBackoff(string name, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
			: this(name, retryCount, minBackoff, maxBackoff, deltaBackoff, RetryStrategy.DefaultFirstFastRetry)
		{
		}

		public ExponentialBackoff(string name, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff, bool firstFastRetry)
			: base(name, firstFastRetry)
		{
			this.retryCount = retryCount;
			this.minBackoff = minBackoff;
			this.maxBackoff = maxBackoff;
			this.deltaBackoff = deltaBackoff;
		}

		public override ShouldRetry GetShouldRetry()
		{
			return (ShouldRetry)((int currentRetryCount, Exception lastException, out TimeSpan retryInterval) =>
			{
				if (currentRetryCount < this.retryCount)
				{
					Random random = new Random();
					int num = (int)Math.Min(this.minBackoff.TotalMilliseconds + (double)(int)((Math.Pow(2.0, (double)currentRetryCount) - 1.0) * (double)random.Next((int)(this.deltaBackoff.TotalMilliseconds * 0.8), (int)(this.deltaBackoff.TotalMilliseconds * 1.2))), this.maxBackoff.TotalMilliseconds);
					retryInterval = TimeSpan.FromMilliseconds((double)num);
					return true;
				}
				retryInterval = TimeSpan.Zero;
				return false;
			});
		}
	}
}