using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Teleopti.Ccc.DBManager.Library
{
	public class SqlAzureTransientErrorDetectionStrategyWithTimeouts : ITransientErrorDetectionStrategy
	{
		private readonly ITransientErrorDetectionStrategy innerStrategy = new SqlDatabaseTransientErrorDetectionStrategy();

		public bool IsTransient(Exception ex)
		{
			if (!innerStrategy.IsTransient(ex))
				return IsTransientTimeout(ex);
			return true;
		}

		protected virtual bool IsTransientTimeout(Exception ex)
		{
			if (IsConnectionTimeout(ex))
				return true;
			if (ex.InnerException != null)
				return IsTransientTimeout(ex.InnerException);
			return false;
		}

		protected virtual bool IsConnectionTimeout(Exception ex)
		{
			SqlException sqlException;
			if (ex != null && (sqlException = ex as SqlException) != null)
				return sqlException.Errors.Cast<SqlError>().Any(error =>
				{
					if (error.Number != -2)
						return error.Number == 121;
					return true;
				});
			return false;
		}
	}
}