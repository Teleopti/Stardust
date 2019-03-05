using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentTransactionHooks : ICurrentTransactionHooks, ITransactionHooksScope
	{
		[ThreadStatic]
		private static IEnumerable<ITransactionHook> _threadHooks;
		private readonly IEnumerable<ITransactionHook> _hooks;

		public CurrentTransactionHooks(IEnumerable<ITransactionHook> hooks)
		{
			_hooks = hooks;
		}

		public IEnumerable<ITransactionHook> Current()
		{
			return _threadHooks ?? _hooks;
		}

		private static IDisposable onThisThreadUse(IEnumerable<ITransactionHook> messageSenders)
		{
			_threadHooks = messageSenders;
			return new GenericDisposable(() =>
			{
				_threadHooks = null;
			});
		}

		public IDisposable OnThisThreadExclude<T>()
		{
			return onThisThreadUse(Current().Where(x => x.GetType() != typeof(T)).ToArray());
		}
	}
}