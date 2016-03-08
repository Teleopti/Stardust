using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public interface IProjectionChangedEventBuilder
	{
		IEnumerable<T> Build<T>(ScheduleChangedEventBase message, IScheduleRange range, DateOnlyPeriod realPeriod)
			where T : ProjectionChangedEventBase, new();
	}
}