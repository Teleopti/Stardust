using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class ScheduleChangeForWeekViewEvent : EventWithLogOnContext
	{
		public IPerson Person { get; set; }
		public IDictionary<DateOnly, ScheduleDayReadModel> NewReadModels { get; set; }
	}
}
