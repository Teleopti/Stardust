using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic)]
	public static class RetryManagerSqlExtensions
	{
		public const string DefaultStrategyCommandTechnologyName = "SQL";
		public const string DefaultStrategyConnectionTechnologyName = "SQLConnection";

		public static RetryStrategy GetDefaultSqlCommandRetryStrategy(this RetryManager retryManager)
		{
			if (retryManager == null)
				throw new ArgumentNullException(nameof(retryManager));
			return retryManager.GetDefaultRetryStrategy("SQL");
		}

		public static RetryPolicy GetDefaultSqlCommandRetryPolicy(this RetryManager retryManager)
		{
			if (retryManager == null)
				throw new ArgumentNullException(nameof(retryManager));
			return new RetryPolicy((ITransientErrorDetectionStrategy)new SqlDatabaseTransientErrorDetectionStrategy(), retryManager.GetDefaultSqlCommandRetryStrategy());
		}

		public static RetryStrategy GetDefaultSqlConnectionRetryStrategy(this RetryManager retryManager)
		{
			if (retryManager == null)
				throw new ArgumentNullException(nameof(retryManager));
			try
			{
				return retryManager.GetDefaultRetryStrategy("SQLConnection");
			}
			catch (ArgumentOutOfRangeException)
			{
				return retryManager.GetDefaultRetryStrategy("SQL");
			}
		}

		public static RetryPolicy GetDefaultSqlConnectionRetryPolicy(this RetryManager retryManager)
		{
			if (retryManager == null)
				throw new ArgumentNullException(nameof(retryManager));
			return new RetryPolicy((ITransientErrorDetectionStrategy)new SqlDatabaseTransientErrorDetectionStrategy(), retryManager.GetDefaultSqlConnectionRetryStrategy());
		}
	}
}