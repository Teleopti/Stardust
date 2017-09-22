using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class CheckMoveActivityLayerOverlapFormData
	{
		public List<PersonActivityItem> PersonActivities { get; set; }
		public DateTime StartTime { get; set; }
	}
}