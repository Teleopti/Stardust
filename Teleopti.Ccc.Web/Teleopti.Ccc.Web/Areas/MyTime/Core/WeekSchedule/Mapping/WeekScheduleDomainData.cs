using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class BaseScheduleDomainData
	{
		public DateOnly Date { get; set; }
		public IScheduleColorSource ColorSource { get; set; }
		public TimePeriod MinMaxTime { get; set; }
		public bool TextRequestPermission { get; set; }
		public bool OvertimeAvailabilityPermission { get; set; }
		public bool AbsenceRequestPermission { get; set; }
		public bool OvertimeRequestPermission { get; set; }
		public bool AbsenceReportPermission { get; set; }
		public bool ShiftExchangePermission { get; set; }
		public bool PersonAccountPermission { get; set; }
		public bool ViewPossibilityPermission { get; set; }
		public bool ShiftTradeBulletinBoardPermission { get; set; }
		public bool AsmEnabled { get; set; }
	}

	public class WeekScheduleDomainData : BaseScheduleDomainData
	{
		public IEnumerable<WeekScheduleDayDomainData> Days { get; set; }
		public bool IsCurrentWeek { get; set; }
	}
}