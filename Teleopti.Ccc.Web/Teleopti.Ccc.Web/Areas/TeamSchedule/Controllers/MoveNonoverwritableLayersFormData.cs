using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	public class MoveNonoverwritableLayersFormData
	{
		public Guid[] PersonIds;
		public DateOnly Date { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}