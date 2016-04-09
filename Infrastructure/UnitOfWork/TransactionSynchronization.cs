using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NHibernate.Transaction;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class TransactionSynchronization : ISynchronization
	{
		private readonly ICurrentTransactionHooks _hooks;
		private readonly Lazy<AggregateRootInterceptor> _interceptor;
		private readonly List<Action> _afterCompletion = new List<Action>();

		public Exception Exception;

		public TransactionSynchronization(ICurrentTransactionHooks hooks, Lazy<AggregateRootInterceptor> interceptor)
		{
			_hooks = hooks;
			_interceptor = interceptor;
		}

		public void BeforeCompletion()
		{
		}

		public void AfterCompletion(bool success)
		{
			if (!success) return;

			try
			{
				var modifiedRoots = _interceptor.Value.ModifiedRoots.ToArray();
				_hooks.Current().ForEach(d => d.AfterCompletion(modifiedRoots));
				_afterCompletion.ForEach(f => f.Invoke());
			}
			catch (Exception e)
			{
				Exception = e;
			}
		}

		public void RegisterForAfterCompletion(Action action)
		{
			_afterCompletion.Add(action);
		}
	}
}