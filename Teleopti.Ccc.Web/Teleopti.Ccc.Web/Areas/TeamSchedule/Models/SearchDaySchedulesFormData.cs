using System;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class SearchDaySchedulesFormData: SearchSchedulesFormData
	{
		public bool IsOnlyAbsences { get; set; }
		public TeamScheduleSortOption SortOption { get; set; }
	}
}