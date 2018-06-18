using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic_76181)]
	public abstract class DefaultReliableSql2008ClientDriver<TTransientErrorDetectionStrategy> : ReliableSql2008ClientDriver
		where TTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy, new()
	{
		protected override ReliableSqlConnection CreateReliableConnection()
		{
			const string incremental = "Incremental Retry Strategy";
			const string backoff = "Backoff Retry Strategy";
			var connectionRetry = new ExponentialBackoff(backoff, 10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10), false);
			var commandRetry = new Incremental(incremental, 10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

			var connection = new ReliableSqlConnection(null,
				new RetryPolicy<TTransientErrorDetectionStrategy>(connectionRetry),
				new RetryPolicy<TTransientErrorDetectionStrategy>(commandRetry)
			);
			connection.ConnectionRetryPolicy.Retrying += ConnectionRetryEventHandler();
			connection.CommandRetryPolicy.Retrying += CommandRetryEventHandler();
			return connection;
		}

		/// <summary>
		/// An event handler delegate which will be called on connection retries.
		/// Only override this if you want to explicitly capture connection retries, otherwise override RetryEventHandler
		/// </summary>
		/// <returns>A custom method for handling the retry events</returns>
		protected virtual EventHandler<RetryingEventArgs> ConnectionRetryEventHandler()
		{
			return RetryEventHandler();
		}

		/// <summary>
		/// An event handler delegate which will be called on command retries.
		/// Only override this if you want to explicitly capture command retries, otherwise override RetryEventHandler
		/// </summary>
		/// <returns>A custom method for handling the retry events</returns>
		protected virtual EventHandler<RetryingEventArgs> CommandRetryEventHandler()
		{
			return RetryEventHandler();
		}

		/// <summary>
		/// An event handler delegate which will be called on connection and command retries.
		/// If you override ConnectionRetryEventHandler and CommandRetryEventHandler then this is redundant.
		/// </summary>
		/// <returns>A custom method for handling the retry events</returns>
		protected virtual EventHandler<RetryingEventArgs> RetryEventHandler()
		{
			return null;
		}
	}
}