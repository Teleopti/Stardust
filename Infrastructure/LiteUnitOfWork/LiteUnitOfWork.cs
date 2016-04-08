using System;
using NHibernate;
using NHibernate.Transaction;

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
			_transaction.RegisterSynchronization(new transactionCallback(action));
		}

		private class transactionCallback : ISynchronization
		{
			private readonly Action _callback;

			public transactionCallback(Action callback)
			{
				_callback = callback;
			}

			public void BeforeCompletion()
			{
			}

			public void AfterCompletion(bool success)
			{
				if (success)
				{
					_callback();
				}
			}
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