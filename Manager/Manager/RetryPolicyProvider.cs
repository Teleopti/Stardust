using System;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Stardust.Manager
{
	public class RetryPolicyProvider
	{
		private const int DelaysSeconds = 3;
		private const int MaxRetry = 20; 
		private const int DelaysMilisecondsTimeout = 500;
		private const int MaxRetryTimeout = 1;

		public RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> GetPolicy()
		{
			var fromSeconds = TimeSpan.FromSeconds(DelaysSeconds);
			var policy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(MaxRetry, fromSeconds);
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