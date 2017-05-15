using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class BaseScheduleDomainData
	{
		public DateOnly Date { get; set; }
		public IScheduleColorSource ColorSource { get; set; }
		public TimePeriod MinMaxTime { get; set; }
		public bool AsmPermission { get; set; }
		public bool TextRequestPermission { get; set; }
		public bool OvertimeAvailabilityPermission { get; set; }
		public bool AbsenceRequestPermission { get; set; }
		public bool AbsenceReportPermission { get; set; }
		public bool ShiftExchangePermission { get; set; }
		public bool PersonAccountPermission { get; set; }
		public bool ViewPossibilityPermission { get; set; }
		public bool ShiftTradeBulletinBoardPermission { get; set; }
	}

	public class DayScheduleDomainData : BaseScheduleDomainData
	{
		public WeekScheduleDayDomainData ScheduleDay { get; set; }
		public bool IsCurrentDay { get; set; }

		public int UnReadMessageCount { get; set; }
		public bool ShiftTradeRequestPermission { get; set; }
	}

	public class WeekScheduleDomainData : BaseScheduleDomainData
	{
		public IEnumerable<WeekScheduleDayDomainData> Days { get; set; }
		public bool IsCurrentWeek { get; set; }
	}
}