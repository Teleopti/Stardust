using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleHandlers
{
	public interface IProjectionChangedEventBuilder
	{
		void Build<T>(ScheduleChangedEventBase message, IScheduleRange range, DateOnlyPeriod realPeriod, Action<T> actionForItems)
			where T : ProjectionChangedEventBase, new();
	}
}