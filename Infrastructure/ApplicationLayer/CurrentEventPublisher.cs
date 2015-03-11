using System;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class CurrentEventPublisher : ICurrentEventPublisher, IEventPublisherScope
	{
		[ThreadStatic]
		private static IEventPublisher _threadEventPublisher;
		private readonly IEventPublisher _eventPublisher;

		public CurrentEventPublisher(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public IDisposable OnThisThreadPublishTo(IEventPublisher eventPublisher)
		{
			_threadEventPublisher = eventPublisher;
			return new GenericDisposable(() =>
			{
				_threadEventPublisher = null;
			});
		}

		public IEventPublisher Current()
		{
			return _threadEventPublisher ?? _eventPublisher;
		}
	}
}