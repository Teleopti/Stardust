using System;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Stardust.Manager
{
	public class RetryPolicyProvider
	{
		private const int DelaysSeconds = 1;
		private const int MaxRetry = 150; 

		public RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> GetPolicy()
		{
			var fromSeconds = TimeSpan.FromSeconds(DelaysSeconds);
			var policy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(MaxRetry, fromSeconds);
			return policy;
		}
	}
}