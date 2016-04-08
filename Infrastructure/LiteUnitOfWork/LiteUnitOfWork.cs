using System;
using NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public class LiteUnitOfWork : ILiteUnitOfWork, IDisposable
	{
		private readonly ISession _session;
		private readonly ITransaction _transaction;

		public LiteUnitOfWork(ISession session)
		{
			_session = session;
			_transaction = _session.BeginTransaction();
		}

		public ISQLQuery CreateSqlQuery(string queryString)
		{
			return _session.CreateSQLQuery(queryString);
		}

		public void OnSuccessfulTransaction(Action action)
		{
			_transaction.RegisterSynchronization(new TransactionSynchronization(action));
		}

		public void Commit()
		{
			_transaction.Commit();
		}

		public void Dispose()
		{
			_transaction.Dispose();
			_session.Dispose();
		}
	}
}