using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	public class SqlAzureTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
	{
		private readonly int[] _errorNumbers = new int[7]
		{
			40540,
			10928,
			10929,
			4060,
			40197,
			40501,
			40613
		};
		private readonly SqlDatabaseTransientErrorDetectionStrategy _entLibStrategy = new SqlDatabaseTransientErrorDetectionStrategy();

		public virtual bool IsTransient(Exception ex)
		{
			return this.IsTransientAzureException(ex);
		}

		private bool IsTransientAzureException(Exception ex)
		{
			if (ex == null)
				return false;
			if (!this._entLibStrategy.IsTransient(ex) && !this.IsNewTransientError(ex))
				return this.IsTransientAzureException(ex.InnerException);
			return true;
		}

		private bool IsNewTransientError(Exception ex)
		{
			SqlException sqlException;
			if ((sqlException = ex as SqlException) != null)
				return sqlException.Errors.Cast<SqlError>().Any<SqlError>((Func<SqlError, bool>)(error => ((IEnumerable<int>)this._errorNumbers).Contains<int>(error.Number)));
			return false;
		}
	}
}