using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.DaySchedule
{
	public class DayScheduleViewModel: BaseScheduleViewModel
	{
		public int UnReadMessageCount { get; set; }
		public string Date { get; set; }
		public bool IsToday { get; set; }
		public DayViewModel Schedule { get; set; }
		public ShiftTradeRequestsPeriodViewModel ShiftTradeRequestSetting { get; set; }
	}
}