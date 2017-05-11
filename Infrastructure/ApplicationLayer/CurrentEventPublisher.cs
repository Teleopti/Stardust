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
		private IEventPublisher _defaultEventPublisher;
		private readonly IEventPublisher _eventPublisher;

		public CurrentEventPublisher(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public IDisposable OnAllThreadsPublishTo(IEventPublisher eventPublisher)
		{
			_defaultEventPublisher = eventPublisher;
			return new GenericDisposable(() =>
			{
				_defaultEventPublisher = null;
			});
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
			if (_threadEventPublishers != null && _threadEventPublishers.Count > 0)
				return _threadEventPublishers.Peek();
			if (_defaultEventPublisher != null)
				return _defaultEventPublisher;
			return _eventPublisher;
		}
	}
}