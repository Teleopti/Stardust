using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IEventPublisherScope
	{
		IDisposable OnAllThreadsPublishTo(IEventPublisher eventPublisher);
		IDisposable OnThisThreadPublishTo(IEventPublisher eventPublisher);
	}
}