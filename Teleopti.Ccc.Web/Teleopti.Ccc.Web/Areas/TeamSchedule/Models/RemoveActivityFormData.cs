using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class RemoveActivityItem
	{
		public Guid ActivityId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

	public class RemovePersonActivityItem
	{
		public Guid PersonId { get; set; }
		public List<RemoveActivityItem> Activities { get; set; } 
	}


	public class RemoveActivityFormData
	{
		public List<RemovePersonActivityItem> PersonActivities { get; set; } 
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}