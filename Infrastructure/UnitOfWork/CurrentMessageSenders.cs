using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentMessageSenders : ICurrentMessageSenders, IMessageSendersScope
	{
		private static IEnumerable<IMessageSender> _globalMessageSenders;
		[ThreadStatic]
		private static IEnumerable<IMessageSender> _threadMessageSenders;
		private readonly IEnumerable<IMessageSender> _messageSenders;

		public CurrentMessageSenders(IEnumerable<IMessageSender> messageSenders)
		{
			_messageSenders = messageSenders;
		}

		public IEnumerable<IMessageSender> Current()
		{
			return _threadMessageSenders ?? _globalMessageSenders ?? _messageSenders;
		}

		public IDisposable GloballyUse(IEnumerable<IMessageSender> messageSenders)
		{
			_globalMessageSenders = messageSenders;
			return new GenericDisposable(() =>
			{
				_globalMessageSenders = null;
			});
		}

		public IDisposable OnThisThreadUse(IEnumerable<IMessageSender> messageSenders)
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