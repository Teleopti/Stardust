using System;
using System.Data.SqlClient;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic_76181)]
	public class SqlTransientErrorDetectionStrategyWithTimeouts : SqlAzureTransientErrorDetectionStrategyWithTimeouts
	{
		protected override bool IsConnectionTimeout(Exception ex)
		{
			if (base.IsConnectionTimeout(ex))
				return true;

			if (isNetworkTimeout(ex))
				return true;

			return isForciblyClosedExistingConnection(ex);
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

		private bool isForciblyClosedExistingConnection(Exception exception)
		{
			var sqlException = exception as SqlException;
			if (sqlException != null)
			{
				return sqlException.Errors.Cast<SqlError>().Any(error => error.Message.Contains("existing connection was forcibly closed") || error.Message.Contains("marked by the server as unrecoverable"));
			}
			return false;
		}
	}
}