using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Stardust.Manager
{
	public class RetryPolicyProvider
	{
		private const int _delaysMiliseconds = 100;
		private const int _maxRetry = 3;

		public RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> GetPolicy()
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(_delaysMiliseconds);
			var policy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(_maxRetry, fromMilliseconds);
			return policy;
		}

		public RetryPolicy<SqlAzureTransientErrorDetectionStrategyWithTimeouts> GetPolicyWithTimeout()
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(_delaysMiliseconds);
			var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategyWithTimeouts>(_maxRetry, fromMilliseconds);
			return policy;
		}
	}

	public class SqlAzureTransientErrorDetectionStrategyWithTimeouts : ITransientErrorDetectionStrategy
	{
		public bool IsTransient(Exception ex)
		{
			return IsTransientTimeout(ex);
		}

		protected virtual bool IsTransientTimeout(Exception ex)
		{
			if (IsConnectionTimeout(ex))
				return true;

			return ex.InnerException != null && IsTransientTimeout(ex.InnerException);
		}

		protected virtual bool IsConnectionTimeout(Exception ex)
		{
			// Timeout exception: error code -2
			// http://social.msdn.microsoft.com/Forums/en-US/ssdsgetstarted/thread/7a50985d-92c2-472f-9464-a6591efec4b3/

			// Timeout exception: error code 121
			// http://social.msdn.microsoft.com/Forums/nl-NL/ssdsgetstarted/thread/5e195f94-d4d2-4c2d-8a4e-7d66b4761510

			SqlException sqlException;
			return ex != null
					 && (sqlException = ex as SqlException) != null
					 && sqlException.Errors.Cast<SqlError>().Any(error => error.Number == -2 || error.Number == 121);
		}
	}
}