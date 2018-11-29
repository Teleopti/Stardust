using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping
{
	public class MonthScheduleDomainData
	{
		public DateOnly CurrentDate { get; set; }
		public IEnumerable<MonthScheduleDayDomainData> Days { get; set; }

		public bool AsmEnabled { get; set; }
	}

	public class MonthScheduleDayDomainData
	{
		public IScheduleDay ScheduleDay { get; set; }
		public OccupancyViewModel[] SeatBookingInformation { get; set; }
	}
}