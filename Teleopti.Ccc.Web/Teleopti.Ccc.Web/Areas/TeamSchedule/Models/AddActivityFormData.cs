using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class AddActivityFormData
	{
		public PersonDate[] PersonDates;
		public Guid ActivityId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool MoveConflictLayerAllowed { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}

	public class PersonDate
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
	}
}