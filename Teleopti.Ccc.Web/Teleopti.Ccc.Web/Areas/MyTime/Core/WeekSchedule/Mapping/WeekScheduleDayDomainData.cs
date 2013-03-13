using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
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
	}
}