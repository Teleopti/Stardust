using System;
using NHibernate.Transaction;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	//tested by NHibernateUnitOfWorkCommitCallbackTest
	public class TransactionCallback : ISynchronization
	{
		private readonly Action _callback;

		public TransactionCallback(Action callback)
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