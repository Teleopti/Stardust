using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	

	public class AnalyticsScheduleRepository : IAnalyticsScheduleRepository
	{
		public void PersistFactScheduleRow(IAnalyticsFactScheduleTime analyticsFactScheduleTime,
			AnalyticsFactScheduleDate analyticsFactScheduleDate, AnalyticsFactSchedulePerson personPart)
		{
			
		}

		public void PersistFactScheduleDayCountRow(AnalyticsFactScheduleDayCount dayCount)
		{
			
		}

		public IList<IAnalyticsActivity> Activities()
		{
			return new List<IAnalyticsActivity>();
		}

		public IList<IAnalyticsAbsence> Absences()
		{
			return new List<IAnalyticsAbsence>();
		}
	}

	public class AnalyticsActivity: IAnalyticsActivity
	{
		public int ActivityId { get; set; }
		public Guid ActivityCode { get; set; }
		public bool InPaidTime { get; set; }
		public bool InReadyTime { get; set; }
	}

	public class AnalyticsAbsence : IAnalyticsAbsence
	{
		public int AbsenceId { get; set; }
		public Guid AbsenceCode { get; set; }
		public bool InPaidTime { get; set; }
	}
}