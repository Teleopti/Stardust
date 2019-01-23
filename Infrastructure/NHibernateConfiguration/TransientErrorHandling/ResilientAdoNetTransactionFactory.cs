using NHibernate;
using NHibernate.Engine;
using NHibernate.Engine.Transaction;
using NHibernate.Transaction;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling
{
	/// <summary>
	/// An NHibernate transaction factory that provides retry logic for transient errors when executing transactions.
	/// </summary>
	/// <remarks>
	/// Requires the connection to be a ReliableSqlDbConnection
	/// </remarks>
	public class ResilientAdoNetTransactionFactory : AdoNetTransactionFactory, ITransactionFactory
	{
		public override ITransaction CreateTransaction(ISessionImplementor session)
		{
			return new ResilientAdoTransaction(session);
		}

		/// <summary>
		/// Executes some work in isolation.
		/// </summary>
		/// <param name="session">The NHibernate session</param>
		/// <param name="work">The work to execute</param>
		/// <param name="transacted">Whether or not to wrap the work in a transaction</param>
		public override void ExecuteWorkInIsolation(ISessionImplementor session, IIsolatedWork work, bool transacted)
		{
			var connection = (ResilientSqlDbConnection)session.Connection;

			ResilientAdoTransaction.ExecuteWithRetry(connection,
				() => base.ExecuteWorkInIsolation(session, work, transacted)
			);
		}
	}
}