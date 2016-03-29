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
		private const int _delaysMilisecondsTimeout = 100;
		private const int _maxRetryTimeout = 1;

		public RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> GetPolicy()
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(_delaysMiliseconds);
			var policy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(_maxRetry, fromMilliseconds);
			return policy;
		}

		public RetryPolicy<SqlAzureTransientErrorDetectionStrategyWithTimeouts> GetPolicyWithTimeout()
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(_delaysMilisecondsTimeout);
			var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategyWithTimeouts>(_maxRetryTimeout, fromMilliseconds);
			return policy;
		}
	}

	public class SqlAzureTransientErrorDetectionStrategyWithTimeouts : SqlAzureTransientErrorDetectionStrategy
	{
		public override bool IsTransient(Exception ex)
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

	public class SqlAzureTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
	{
		// From Enterprise Library 6 changelog (see https://entlib.codeplex.com/wikipage?title=EntLib6ReleaseNotes):
		// Error code 40540 from SQL Database added as a transient error (see http://msdn.microsoft.com/en-us/library/ff394106.aspx#bkmk_throt_errors).
		// Added error codes 10928 and 10929 from SQL Database as transient errors (see http://blogs.msdn.com/b/psssql/archive/2012/10/31/worker-thread-governance-coming-to-azure-sql-database.aspx).
		// Added error codes 4060, 40197, 40501, 40613 from MSDN documentation (see https://azure.microsoft.com/en-us/documentation/articles/sql-database-develop-error-messages/)

		private readonly int[] _errorNumbers = new int[] { 40540, 10928, 10929, 4060, 40197, 40501, 40613 };

		private readonly SqlDatabaseTransientErrorDetectionStrategy _entLibStrategy = new SqlDatabaseTransientErrorDetectionStrategy();

		public virtual bool IsTransient(Exception ex)
		{
			return IsTransientAzureException(ex);
		}

		private bool IsTransientAzureException(Exception ex)
		{
			if (ex == null)
				return false;

			return _entLibStrategy.IsTransient(ex)
				|| IsNewTransientError(ex)
				|| IsTransientAzureException(ex.InnerException);
		}

		private bool IsNewTransientError(Exception ex)
		{
			SqlException sqlException;
			return (sqlException = ex as SqlException) != null
				   && sqlException.Errors.Cast<SqlError>().Any(error => _errorNumbers.Contains(error.Number));
		}
	}
}