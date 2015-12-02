using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class PagingGroupScheduleShiftViewModel
	{
		public IEnumerable<GroupScheduleShiftViewModel> GroupSchedule { get; set; }
		public int TotalPages { get; set; }
	}

	public class GroupScheduleViewModel
	{
		public IEnumerable<GroupScheduleShiftViewModel> Schedules { get; set; }
		public int Total { get; set; }
		public string Keyword { get; set; }
	}
}