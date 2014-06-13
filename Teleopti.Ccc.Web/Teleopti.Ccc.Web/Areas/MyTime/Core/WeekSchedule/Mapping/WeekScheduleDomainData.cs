using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class WeekScheduleDomainData
	{
		public DateOnly Date { get; set; }
		public IEnumerable<WeekScheduleDayDomainData> Days { get; set; }
		public IScheduleColorSource ColorSource { get; set; }
		public TimePeriod MinMaxTime { get; set; }
		public bool AsmPermission { get; set; }
		public bool AbsenceRequestPermission { get; set; }
        public bool IsCurrentWeek { get; set; }

	}
}