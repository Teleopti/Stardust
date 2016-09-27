using System.Collections.Generic;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GroupScheduleShiftViewModel
	{
		public string PersonId { get; set; }
		public string Name { get; set; }
		public string Date { get; set; }
		public double WorkTimeMinutes { get; set; }
		public double ContractTimeMinutes { get; set; }

		public IEnumerable<GroupScheduleProjectionViewModel> Projection { get; set; }

		public ShiftCategoryViewModel ShiftCategory { get; set; }
		public bool IsFullDayAbsence { get; set; }
		public GroupScheduleDayOffViewModel DayOff { get; set; }
		public string InternalNotes { get; set; }
	}
}