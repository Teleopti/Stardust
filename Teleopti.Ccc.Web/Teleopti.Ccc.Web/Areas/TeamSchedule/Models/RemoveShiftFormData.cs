using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class RemoveShiftFormData
	{
		public DateOnly Date { get; set; }
		public Guid[] PersonIds { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}