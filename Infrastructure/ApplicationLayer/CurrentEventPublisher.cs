using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class CurrentEventPublisher : ICurrentEventPublisher, IEventPublisherScope
	{
		[ThreadStatic]
		private static Stack<IEventPublisher> _threadEventPublishers;
		private readonly IEventPublisher _eventPublisher;

		public CurrentEventPublisher(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public IDisposable OnThisThreadPublishTo(IEventPublisher eventPublisher)
		{
			if (_threadEventPublishers == null)
				_threadEventPublishers = new Stack<IEventPublisher>();
			_threadEventPublishers.Push(eventPublisher);
			return new GenericDisposable(() =>
			{
				_threadEventPublishers.Pop();
			});
		}

		public IEventPublisher Current()
		{
			if (_threadEventPublishers == null) return _eventPublisher;
			if (_threadEventPublishers.Count == 0) return _eventPublisher;
			return _threadEventPublishers.Peek();
		}
	}
}