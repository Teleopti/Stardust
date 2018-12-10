using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class PersonActivityItem
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public IList<Guid> ShiftLayerIds { get; set; }
	}


	public class MoveActivityFormData
	{
		public List<PersonActivityItem> PersonActivities { get; set; }
		public DateTime StartTime { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}