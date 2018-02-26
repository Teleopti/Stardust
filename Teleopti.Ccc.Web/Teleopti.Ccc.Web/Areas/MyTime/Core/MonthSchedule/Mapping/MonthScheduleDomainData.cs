using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping
{
	public class MonthScheduleDomainData
	{
		public DateOnly CurrentDate { get; set; }
		public IEnumerable<MonthScheduleDayDomainData> Days { get; set; }

		public bool AsmPermission { get; set; }
	}

	public class MonthScheduleDayDomainData
	{
		public IScheduleDay ScheduleDay { get; set; }
		public OccupancyViewModel[] SeatBookingInformation { get; set; }
	}
}