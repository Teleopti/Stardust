using System;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Stardust.Manager
{
	public class RetryPolicyProvider
	{
		private const int DelaysMiliseconds = 100;
		private const int MaxRetry = 3;
		private const int DelaysMilisecondsTimeout = 100;
		private const int MaxRetryTimeout = 1;

		public RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> GetPolicy()
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(DelaysMiliseconds);
			var policy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(MaxRetry, fromMilliseconds);
			return policy;
		}

		public RetryPolicy<SqlAzureTransientErrorDetectionStrategyWithTimeouts> GetPolicyWithTimeout()
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(DelaysMilisecondsTimeout);
			var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategyWithTimeouts>(MaxRetryTimeout, fromMilliseconds);
			return policy;
		}
	}
}