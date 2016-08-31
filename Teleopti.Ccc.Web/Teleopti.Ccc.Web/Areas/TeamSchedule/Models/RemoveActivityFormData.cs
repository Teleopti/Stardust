using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class PersonActivityItem
	{
		public Guid PersonId { get; set; }
		public IList<Guid> ShiftLayerIds { get; set; } 
	}

	public class RemoveActivityFormData
	{
		public IList<PersonActivityItem> PersonActivities { get; set; } 
		public DateOnly Date { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}