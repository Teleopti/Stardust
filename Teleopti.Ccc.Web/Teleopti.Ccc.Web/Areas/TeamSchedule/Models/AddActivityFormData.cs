using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class AddActivityFormData
	{
		public Guid[] PersonIds;
		public Guid ActivityId { get; set; }
		public DateOnly BelongsToDate { get; set; }
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}