using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class TeamScheduleShiftViewModel
	{
		public string PersonId { get; set; }
		public int WorkTimeMinutes { get; set; }
		public int ContractTimeMinutes { get; set; }
		public IEnumerable<TeamScheduleLayerViewModel> Projection { get; set; }
		public bool IsFullDayAbsence { get; set; }
		public TeamScheduleDayOffViewModel DayOff { get; set; }
	}
}