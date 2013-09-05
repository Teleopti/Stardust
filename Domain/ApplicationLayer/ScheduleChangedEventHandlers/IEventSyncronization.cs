using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public interface IEventSyncronization
	{
		void WhenDone(Action done);
	}
}