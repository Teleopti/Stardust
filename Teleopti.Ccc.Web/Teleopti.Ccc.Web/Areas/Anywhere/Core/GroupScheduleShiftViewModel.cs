using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GroupScheduleShiftViewModel
	{
		public string PersonId { get; set; }
		public int WorkTimeMinutes { get; set; }
		public int ContractTimeMinutes { get; set; }
		public IEnumerable<GroupScheduleLayerViewModel> Projection { get; set; }
		public bool IsFullDayAbsence { get; set; }
		public GroupScheduleDayOffViewModel DayOff { get; set; }
	}
}