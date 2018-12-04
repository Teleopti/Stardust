using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Transaction;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class TransactionSynchronization : ITransactionCompletionSynchronization
	{
		private readonly ICurrentTransactionHooks _hooks;
		private readonly NHibernateUnitOfWorkInterceptor _interceptor;
		private readonly List<Action> _afterCompletion = new List<Action>();

		public Exception Exception;

		public TransactionSynchronization(ICurrentTransactionHooks hooks, NHibernateUnitOfWorkInterceptor interceptor)
		{
			_hooks = hooks;
			_interceptor = interceptor;
		}


		public void RegisterForAfterCompletion(Action action)
		{
			_afterCompletion.Add(action);
		}

		public void ExecuteBeforeTransactionCompletion()
		{
		}

		public Task ExecuteBeforeTransactionCompletionAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public void ExecuteAfterTransactionCompletion(bool success)
		{
			if (!success) return;

			try
			{
				var modifiedRoots = _interceptor.ModifiedRoots.ToArray();
				_hooks.Current().ForEach(d => d.AfterCompletion(modifiedRoots));
				_afterCompletion.ForEach(f => f.Invoke());
			}
			catch (Exception e)
			{
				Exception = e;
			}
		}

		public Task ExecuteAfterTransactionCompletionAsync(bool success, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}