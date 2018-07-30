using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule
{
	public class WeekScheduleViewModel : BaseScheduleViewModel
	{
		public PeriodSelectionViewModel PeriodSelection { get; set; }
		public IEnumerable<DayViewModel> Days { get; set; }
		public bool IsCurrentWeek { get; set; }
		public string CurrentWeekEndDate { get; set; }
		public string CurrentWeekStartDate { get; set; }
		public IEnumerable<StyleClassViewModel> Styles { get; set; }
	}

	public class PersonDayOffPeriodViewModel : PeriodViewModel
	{
	}

	public class PersonAssignmentPeriodViewModel : PeriodViewModel
	{
	}

	public class FullDayAbsencePeriodViewModel : PeriodViewModel
	{
	}
}