using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class MovePersonActivityItem
	{
		public Guid PersonId { get; set; }
		public List<Guid> ShiftLayerIds { get; set; }
	}


	public class MoveActivityFormData
	{
		public List<RemovePersonActivityItem> PersonActivities { get; set; }
		public DateTime StartTime { get; set; }
		public DateOnly Date { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}