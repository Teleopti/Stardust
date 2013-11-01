using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class WeekScheduleDayDomainData
	{
		public DateOnly Date { get; set; }

		public IEnumerable<IPersonRequest> PersonRequests { get; set; }
		public IScheduleDay ScheduleDay { get; set; }
		public IVisualLayerCollection Projection { get; set; }
        public IVisualLayerCollection ProjectionYesterday { get; set; }
        public TimePeriod MinMaxTime { get; set; }
		public double Allowance { get; set; }
		public double AbsenceTime { get; set; }
		public bool Availability { get; set; }
		public double FulltimeEquivalent { get; set; }
		public IOvertimeAvailability OvertimeAvailability { get; set; }
		public IOvertimeAvailability OvertimeAvailabilityYesterday { get; set; }
		public string ProbabilityClass { get; set; }
		public string ProbabilityText { get; set; }
	}
}