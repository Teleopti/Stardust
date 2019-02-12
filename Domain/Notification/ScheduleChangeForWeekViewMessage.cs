using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class ScheduleChangeForWeekViewMessage
	{
		public string LogOnDatasource { get; set; }
		public Guid LogOnBusinessUnitId { get; set; }
		public IPerson Person { get; set; }
		public IDictionary<DateOnly, ScheduleDayReadModel> NewReadModels { get; set; }
	}
}
