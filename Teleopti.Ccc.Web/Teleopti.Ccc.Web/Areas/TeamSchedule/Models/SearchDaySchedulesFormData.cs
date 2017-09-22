using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class SearchDaySchedulesFormData: SearchSchedulesFormData
	{
		public bool IsOnlyAbsences { get; set; }
		public TeamScheduleSortOption SortOption { get; set; }
	}
}