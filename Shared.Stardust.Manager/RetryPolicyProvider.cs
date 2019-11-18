using System;
using System.Data.SqlClient;
using Polly;
using Polly.Retry;

namespace Stardust.Manager
{
	public class RetryPolicyProvider
	{
		private const int DelaysSeconds = 1;
		private const int MaxRetry = 150; 

		public RetryPolicy GetPolicy()
		{
			var fromSeconds = TimeSpan.FromSeconds(DelaysSeconds);
            var policy = Policy.Handle<TimeoutException>()
                .Or<SqlException>(DetectTransientSqlException.IsTransient)
                .OrInner<SqlException>(DetectTransientSqlException.IsTransient)
                .WaitAndRetry(MaxRetry, _ => fromSeconds);

            return policy;
		}
	}
}