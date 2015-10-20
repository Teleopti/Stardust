using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentPersistCallbacks : ICurrentPersistCallbacks, IMessageSendersScope
	{
		private static IEnumerable<IPersistCallback> _globalMessageSenders;
		[ThreadStatic]
		private static IEnumerable<IPersistCallback> _threadMessageSenders;
		private readonly IEnumerable<IPersistCallback> _messageSenders;

		public CurrentPersistCallbacks(IEnumerable<IPersistCallback> messageSenders)
		{
			_messageSenders = messageSenders;
		}

		public IEnumerable<IPersistCallback> Current()
		{
			return _threadMessageSenders ?? _globalMessageSenders ?? _messageSenders;
		}

		public IDisposable GloballyUse(IEnumerable<IPersistCallback> messageSenders)
		{
			_globalMessageSenders = messageSenders;
			return new GenericDisposable(() =>
			{
				_globalMessageSenders = null;
			});
		}

		public IDisposable OnThisThreadUse(IEnumerable<IPersistCallback> messageSenders)
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