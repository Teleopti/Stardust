using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class ChangeActivityTypeFormData
	{

		public Guid PersonId { get; set; }

		public DateTime Date { get; set; }

		public EditingLayerModel[] Layers { get; set; }


	}
	public class EditingLayerModel
	{
		public Guid ActivityId { get; set; }
		public Guid[] ShiftLayerIds { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }

		public bool IsNew { get; set; }
	}
}