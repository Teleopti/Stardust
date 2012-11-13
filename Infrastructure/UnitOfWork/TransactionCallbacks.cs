using System;
using NHibernate.Transaction;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	//tested by NHibernateUnitOfWorkCommitCallbackTest
	public class TransactionCallbacks : ISynchronization
	{
		private readonly Action _callback;

		public TransactionCallbacks(Action callback)
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
}