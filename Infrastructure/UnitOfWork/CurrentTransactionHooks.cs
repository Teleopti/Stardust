using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentTransactionHooks : ICurrentTransactionHooks, ITransactionHooksScope
	{
		private static IEnumerable<ITransactionHook> _globalMessageSenders;
		[ThreadStatic]
		private static IEnumerable<ITransactionHook> _threadMessageSenders;
		private readonly IEnumerable<ITransactionHook> _messageSenders;

		public CurrentTransactionHooks(IEnumerable<ITransactionHook> messageSenders)
		{
			_messageSenders = messageSenders;
		}

		public IEnumerable<ITransactionHook> Current()
		{
			return _threadMessageSenders ?? _globalMessageSenders ?? _messageSenders;
		}

		public IDisposable GloballyUse(IEnumerable<ITransactionHook> messageSenders)
		{
			_globalMessageSenders = messageSenders;
			return new GenericDisposable(() =>
			{
				_globalMessageSenders = null;
			});
		}

		public IDisposable OnThisThreadUse(IEnumerable<ITransactionHook> messageSenders)
		{
			_threadMessageSenders = messageSenders;
			return new GenericDisposable(() =>
			{
				_threadMessageSenders = null;
			});
		}

		public IDisposable OnThisThreadExclude<T>()
		{
			return OnThisThreadUse(Current().Where(x => x.GetType() != typeof(T)).ToArray());
		}
	}
}