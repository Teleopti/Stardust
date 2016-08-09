using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class CheckMoveActivityLayerOverlapFormData
	{
		public List<PersonActivityItem> PersonActivities { get; set; }
		public DateTime StartTime { get; set; }
		public DateOnly Date { get; set; }
	}
}