using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	public class FixedInterval : RetryStrategy
	{
		private readonly int retryCount;
		private readonly TimeSpan retryInterval;

		public FixedInterval()
			: this(RetryStrategy.DefaultClientRetryCount)
		{
		}

		public FixedInterval(int retryCount)
			: this(retryCount, RetryStrategy.DefaultRetryInterval)
		{
		}

		public FixedInterval(int retryCount, TimeSpan retryInterval)
			: this((string)null, retryCount, retryInterval, RetryStrategy.DefaultFirstFastRetry)
		{
		}

		public FixedInterval(string name, int retryCount, TimeSpan retryInterval)
			: this(name, retryCount, retryInterval, RetryStrategy.DefaultFirstFastRetry)
		{
		}

		public FixedInterval(string name, int retryCount, TimeSpan retryInterval, bool firstFastRetry)
			: base(name, firstFastRetry)
		{
			this.retryCount = retryCount;
			this.retryInterval = retryInterval;
		}

		public override ShouldRetry GetShouldRetry()
		{
			if (this.retryCount == 0)
				return (ShouldRetry)((int currentRetryCount, Exception lastException, out TimeSpan interval) =>
				{
					interval = TimeSpan.Zero;
					return false;
				});
			return (ShouldRetry)((int currentRetryCount, Exception lastException, out TimeSpan interval) =>
			{
				if (currentRetryCount < this.retryCount)
				{
					interval = this.retryInterval;
					return true;
				}
				interval = TimeSpan.Zero;
				return false;
			});
		}
	}
}