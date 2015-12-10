using System;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.SqlAzure.RetryStrategies;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	[CLSCompliant(false)]
	public class SqlTransientErrorDetectionStrategyWithTimeouts : SqlAzureTransientErrorDetectionStrategyWithTimeouts
	{
		protected override bool IsConnectionTimeout(Exception ex)
		{
			if (base.IsConnectionTimeout(ex))
				return true;

			return isNetworkTimeout(ex);
		}

		private bool isNetworkTimeout(Exception exception)
		{
			var sqlException = exception as SqlException;
			if (sqlException != null)
			{
				return sqlException.Errors.Cast<SqlError>().Any(error => error.Number == 40);
			}
			return false;
		}
	}
}