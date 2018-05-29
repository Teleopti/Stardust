using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule
{
	public class TeamScheduleRequest
	{
		public DateTime SelectedDate { get; set; }
		public ScheduleFilter ScheduleFilter { get; set; }
		public Paging Paging { get; set; }
	}
}