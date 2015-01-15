using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface ICurrentEventPublisher
	{
		IEventPublisher Current();
	}

	public interface ICurrentEventPublisherContext
	{
		void PublishTo(IEventPublisher eventPublisher);
	}

	public class CurrentEventPublisher : ICurrentEventPublisher, ICurrentEventPublisherContext
	{
		private IEventPublisher _eventPublisher;

		public CurrentEventPublisher(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void PublishTo(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public IEventPublisher Current()
		{
			return _eventPublisher;
		}
	}
}