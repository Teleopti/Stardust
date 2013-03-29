using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public interface IProjectionChangedEventBuilder
	{
		void Build<T>(ScheduleChangedEventBase message, IScheduleRange range, DateOnlyPeriod realPeriod, Action<T> actionForItems)
			where T : ProjectionChangedEventBase, new();
	}
}