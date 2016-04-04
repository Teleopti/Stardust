using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Stardust.Manager
{
	public class SqlAzureTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
	{
		private readonly SqlDatabaseTransientErrorDetectionStrategy _entLibStrategy =
			new SqlDatabaseTransientErrorDetectionStrategy();

		// From Enterprise Library 6 changelog (see https://entlib.codeplex.com/wikipage?title=EntLib6ReleaseNotes):
		// Error code 40540 from SQL Database added as a transient error (see http://msdn.microsoft.com/en-us/library/ff394106.aspx#bkmk_throt_errors).
		// Added error codes 10928 and 10929 from SQL Database as transient errors (see http://blogs.msdn.com/b/psssql/archive/2012/10/31/worker-thread-governance-coming-to-azure-sql-database.aspx).
		// Added error codes 4060, 40197, 40501, 40613 from MSDN documentation (see https://azure.microsoft.com/en-us/documentation/articles/sql-database-develop-error-messages/)

		private readonly int[] _errorNumbers = {40540, 10928, 10929, 4060, 40197, 40501, 40613};

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