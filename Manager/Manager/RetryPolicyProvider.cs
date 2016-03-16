using System;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Stardust.Manager.Helpers;

namespace Stardust.Manager
{
	public class RetryPolicyProvider
	{
		private const int _delaysMiliseconds = 100;
		private const int _maxRetry = 3;

		public RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> GetPolicy(ILog logger)
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(_delaysMiliseconds);
			var policy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(_maxRetry, fromMilliseconds);
			policy.Retrying += (sender, args) =>
			{
				// Log details of the retry.
				var msg = String.Format("Retry - Count:{0}, Delay:{1}, Exception:{2}", args.CurrentRetryCount, args.Delay, args.LastException);
				LogHelper.LogErrorWithLineNumber(logger, msg);
			};
			return policy;
		}
	}
}