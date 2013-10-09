using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class TeamScheduleShiftViewModel
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Id { get; set; }
		public int WorkTimeMinutes { get; set; }
		public int ContractTimeMinutes { get; set; }
		public IEnumerable<TeamScheduleLayerViewModel> Projection { get; set; }
		public bool IsFullDayAbsence { get; set; }
		public string DayOffStartTime { get; set; }
		public string DayOffEndTime { get; set; }
	}
}