using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public interface IProjectionChangedEventBuilder
	{
		IEnumerable<T> Build<T>(ScheduleChangedEventBase message, IScheduleRange range, DateOnlyPeriod realPeriod, IEnumerable<ProjectionVersion> versions)
			where T : ProjectionChangedEventBase, new();

		IEnumerable<ProjectionChangedEventLayer> BuildProjectionChangedEventLayers(IVisualLayerCollection projection, IPerson person);
		ProjectionChangedEventScheduleDay BuildEventScheduleDay(IScheduleDay scheduleDay);
	}
}