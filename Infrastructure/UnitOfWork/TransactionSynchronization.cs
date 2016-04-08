using System;
using System.Collections.Generic;
using NHibernate.Transaction;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class TransactionSynchronization : ISynchronization
	{
		private readonly ICurrentPersistCallbacks _callbacks;
		private readonly Lazy<AggregateRootInterceptor> _interceptor;
		private readonly List<Action> _afterCompletion = new List<Action>();
		 
		public TransactionSynchronization(ICurrentPersistCallbacks callbacks, Lazy<AggregateRootInterceptor> interceptor)
		{
			_callbacks = callbacks;
			_interceptor = interceptor;
		}

		public void BeforeCompletion()
		{
		}

		public void AfterCompletion(bool success)
		{
			if (!success) return;
			_callbacks.Current().ForEach(d => d.AfterCommit(_interceptor.Value.ModifiedRoots));
			_afterCompletion.ForEach(f => f.Invoke());
		}

		public void RegisterForAfterCompletion(Action func)
		{
			_afterCompletion.Add(func);
		}
	}
}