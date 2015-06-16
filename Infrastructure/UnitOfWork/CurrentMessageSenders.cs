using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentMessageSenders : ICurrentMessageSenders, IMessageSendersScope
	{
		private static IEnumerable<IMessageSender> _globalMessageSenders;
		private readonly IEnumerable<IMessageSender> _messageSenders;

		public CurrentMessageSenders(IEnumerable<IMessageSender> messageSenders)
		{
			_messageSenders = messageSenders;
		}

		public IEnumerable<IMessageSender> Current()
		{
			return _globalMessageSenders ?? _messageSenders;
		}

		public IDisposable GloballyUse(IEnumerable<IMessageSender> messageSenders)
		{
			_globalMessageSenders = messageSenders;
			return new GenericDisposable(() =>
			{
				_globalMessageSenders = null;
			});
		}
	}
}