using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	public class Incremental : RetryStrategy
	{
		private readonly int retryCount;
		private readonly TimeSpan initialInterval;
		private readonly TimeSpan increment;

		public Incremental()
			: this(RetryStrategy.DefaultClientRetryCount, RetryStrategy.DefaultRetryInterval, RetryStrategy.DefaultRetryIncrement)
		{
		}

		public Incremental(int retryCount, TimeSpan initialInterval, TimeSpan increment)
			: this((string)null, retryCount, initialInterval, increment)
		{
		}

		public Incremental(string name, int retryCount, TimeSpan initialInterval, TimeSpan increment)
			: this(name, retryCount, initialInterval, increment, RetryStrategy.DefaultFirstFastRetry)
		{
		}

		public Incremental(string name, int retryCount, TimeSpan initialInterval, TimeSpan increment, bool firstFastRetry)
			: base(name, firstFastRetry)
		{
			this.retryCount = retryCount;
			this.initialInterval = initialInterval;
			this.increment = increment;
		}

		public override ShouldRetry GetShouldRetry()
		{
			return (ShouldRetry)((int currentRetryCount, Exception lastException, out TimeSpan retryInterval) =>
			{
				if (currentRetryCount < this.retryCount)
				{
					retryInterval = TimeSpan.FromMilliseconds(this.initialInterval.TotalMilliseconds + this.increment.TotalMilliseconds * (double)currentRetryCount);
					return true;
				}
				retryInterval = TimeSpan.Zero;
				return false;
			});
		}
	}
}