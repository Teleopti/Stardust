using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class MoveActivityFormData
	{
		public List<PersonActivityItem> PersonActivities { get; set; }
		public DateTime StartTime { get; set; }
		public DateOnly Date { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}