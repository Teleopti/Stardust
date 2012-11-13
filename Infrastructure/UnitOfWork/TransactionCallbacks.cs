using System;
using NHibernate.Transaction;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	//tested by NHibernateUnitOfWorkCommitCallbackTest
	public class TransactionCallbacks : ISynchronization
	{
		private readonly Action<object> _callback;

		public TransactionCallbacks(Action<object> callback)
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
				_callback(null);
			}
		}
	}
}