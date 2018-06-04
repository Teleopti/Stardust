using System;
using System.Data.Common;
using System.Data.SqlClient;
using NHibernate.AdoNet;
using NHibernate.Driver;
using Polly;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
	/// <summary>
	/// Abstract base class that enables the creation of an NHibernate client driver that extends the Sql 2008 driver,
	/// but adds in transient fault handling retry logic via <see cref="ResilientSqlConnection"/>.
	/// </summary>
	public class ResilientSql2008ClientDriver : Sql2008ClientDriver, IEmbeddedBatcherFactoryProvider
	{
		/// <summary>
		/// Provides a <see cref="ResilientSqlConnection"/> instance to use for connections.
		/// </summary>
		/// <returns>A reliable connection</returns>
		protected ResilientSqlConnection CreateResilientConnection()
		{
			var connectionRetry = Policy.Handle<TimeoutException>()
				.Or<SqlException>(DetectTransientSqlException.IsTransient)
				.OrInner<SqlException>(DetectTransientSqlException.IsTransient)
				.WaitAndRetry(10, i => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(i, 2))), (ex,t,c) => ConnectionRetryEventHandler());

			var commandRetry = Policy.Handle<TimeoutException>()
				.Or<SqlException>(DetectTransientSqlException.IsTransient)
				.OrInner<SqlException>(DetectTransientSqlException.IsTransient)
				.WaitAndRetry(10, i => TimeSpan.FromSeconds(i), (ex, t, c) => CommandRetryEventHandler());

			return new ResilientSqlConnection(null, connectionRetry, commandRetry);
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

		/// <summary>
		/// Creates an uninitialized <see cref="T:System.Data.IDbConnection"/> object for the SqlClientDriver.
		/// </summary>
		/// <value>
		/// An unitialized <see cref="T:System.Data.SqlClient.SqlConnection"/> object.
		/// </value>
		public override DbConnection CreateConnection()
		{
			return new ResilientSqlDbConnection(CreateResilientConnection());
		}

		/// <summary>
		/// Creates an uninitialized <see cref="T:System.Data.IDbCommand"/> object for the SqlClientDriver.
		/// </summary>
		/// <value>
		/// An unitialized <see cref="T:System.Data.SqlClient.SqlCommand"/> object.
		/// </value>
		public override DbCommand CreateCommand()
		{
			return new ResilientSqlCommand();
		}

		/// <summary>
		/// Returns the class to use for the Batcher Factory.
		/// </summary>
		public System.Type BatcherFactoryClass => typeof(ResilientSqlClientBatchingBatcherFactory);
	}
}