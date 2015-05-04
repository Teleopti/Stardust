using System;
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
		public MinMaxTimePeriod MinMaxTimeLineData { get; set; }
		public bool AsmPermission { get; set; }
		public bool TextRequestPermission { get; set; }
		public bool OvertimeAvailabilityPermission { get; set; }
		public bool AbsenceRequestPermission { get; set; }
		public bool AbsenceReportPermission { get; set; }
		public bool ShiftExchangePermission { get; set; }
		public bool PersonAccountPermission { get; set; }
		public bool IsCurrentWeek { get; set; }
		public bool ShiftTradeBulletinBoardPermission { get; set; }	
	}

	public class MinMaxTimePeriod
	{
		public TimePeriod MinMaxTime { get; set; }
		public bool isContainsEndDateDST { get; set; }
		public TimeSpan timespanDST { get; set; }
	}
}