using System.Data;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Transaction;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
	/// <summary>
	/// Provides a transaction implementation that includes transient fault-handling retry logic.
	/// </summary>
	public class ResilientAdoTransaction : AdoTransaction, ITransaction
	{
		private readonly ISessionImplementor _session;

		/// <summary>
		/// Constructs a <see cref="ResilientAdoTransaction"/>.
		/// </summary>
		/// <param name="session">NHibernate session to use.</param>
		public ResilientAdoTransaction(ISessionImplementor session) : base(session)
		{
			_session = session;
		}

		public new void Begin()
		{
			Begin(IsolationLevel.Unspecified);
		}

		public new void Begin(IsolationLevel isolationLevel)
		{
			ExecuteWithRetry(_session.Connection as ResilientSqlDbConnection, () => base.Begin(isolationLevel));
		}

		/// <summary>
		/// Executes the given action with the command retry policy on the given <see cref="ResilientSqlDbConnection"/>.
		/// </summary>
		/// <param name="connection">The reliable connection</param>
		/// <param name="action">The action to execute</param>
		public static void ExecuteWithRetry(ResilientSqlDbConnection connection, System.Action action)
		{
			connection.ReliableConnection.CommandRetryPolicy.Execute(() =>
				{
					if (connection.State != ConnectionState.Open)
						connection.Open();

					action();
				}
			);
		}
	}
}