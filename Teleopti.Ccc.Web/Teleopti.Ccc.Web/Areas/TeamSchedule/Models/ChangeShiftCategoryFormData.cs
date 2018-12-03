using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class ChangeShiftCategoryFormData
	{
		public List<Guid> PersonIds { get; set; }
		public DateOnly Date { get; set; }
		public Guid ShiftCategoryId { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}