using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	public class EditScheduleNoteFormData
	{
		public Guid PersonId { get; set; }
		public DateOnly SelectedDate { get; set; }
		public string InternalNote { get; set; }
		public string PublicNote { get; set; }
	}
}