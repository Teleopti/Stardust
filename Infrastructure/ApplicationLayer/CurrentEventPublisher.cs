using System;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface ICurrentEventPublisher
	{
		IEventPublisher Current();
	}

	public interface ICurrentEventPublisherScope
	{
		IDisposable PublishTo(IEventPublisher eventPublisher);
	}

	public class CurrentEventPublisher : ICurrentEventPublisher, ICurrentEventPublisherScope
	{
		[ThreadStatic]
		private static IEventPublisher _threadEventPublisher;
		private readonly IEventPublisher _eventPublisher;

		public CurrentEventPublisher(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public IDisposable PublishTo(IEventPublisher eventPublisher)
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