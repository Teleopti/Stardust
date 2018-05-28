using System;
using System.Data.SqlClient;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	public class SqlAzureTransientErrorDetectionStrategyWithTimeouts : SqlAzureTransientErrorDetectionStrategy
	{
		public override bool IsTransient(Exception ex)
		{
			if (!base.IsTransient(ex))
				return this.IsTransientTimeout(ex);
			return true;
		}

		protected virtual bool IsTransientTimeout(Exception ex)
		{
			if (this.IsConnectionTimeout(ex))
				return true;
			if (ex.InnerException != null)
				return this.IsTransientTimeout(ex.InnerException);
			return false;
		}

		protected virtual bool IsConnectionTimeout(Exception ex)
		{
			SqlException sqlException;
			if (ex != null && (sqlException = ex as SqlException) != null)
				return sqlException.Errors.Cast<SqlError>().Any<SqlError>((Func<SqlError, bool>)(error =>
				{
					if (error.Number != -2)
						return error.Number == 121;
					return true;
				}));
			return false;
		}
	}
}