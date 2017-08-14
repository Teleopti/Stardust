using System;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class SearchWeekSchedulesFormData
	{
		public Guid[] SelectedTeamIds { get; set; }
		public string Keyword { get; set; }
		public DateTime Date { get; set; }
		public int PageSize { get; set; }
		public int CurrentPageIndex { get; set; }
		public Guid[] SelectedGroupIds { get; set; }
	}
}