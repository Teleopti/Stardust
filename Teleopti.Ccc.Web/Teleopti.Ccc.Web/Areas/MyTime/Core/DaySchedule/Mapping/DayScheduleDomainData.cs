using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.Mapping
{
	public class DayScheduleDomainData
	{
		public DateOnly Date { get; set; }
		public IScheduleColorSource ColorSource { get; set; }
		public TimePeriod MinMaxTime { get; set; }
		public bool AsmEnabled { get; set; }
		public int PersonRequestCount { get; set; }
		public IScheduleDay ScheduleDay { get; set; }
		public IVisualLayerCollection Projection { get; set; }
		public IVisualLayerCollection ProjectionYesterday { get; set; }
		public bool Availability { get; set; }
		public IOvertimeAvailability OvertimeAvailability { get; set; }
		public IOvertimeAvailability OvertimeAvailabilityYesterday { get; set; }
		public string ProbabilityClass { get; set; }
		public string ProbabilityText { get; set; }
		public IList<OccupancyViewModel> SeatBookingInformation { get; set; }
		public bool IsCurrentDay { get; set; }
		public int UnReadMessageCount { get; set; }
		public bool TextRequestPermission { get; set; }
		public bool OvertimeAvailabilityPermission { get; set; }
		public bool AbsenceRequestPermission { get; set; }
		public bool OvertimeRequestPermission { get; set; }
		public bool AbsenceReportPermission { get; set; }
		public bool ShiftExchangePermission { get; set; }
		public bool PersonAccountPermission { get; set; }
		public bool ViewPossibilityPermission { get; set; }
		public bool ShiftTradeRequestPermission { get; set; }
		public bool ShiftTradeBulletinBoardPermission { get; set; }
	}
}