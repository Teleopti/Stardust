using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class GetSchedulesForPeopleFormData
	{
		public List<Guid> PersonIds { get; set; }
		public DateTime Date { get; set; }
	}
}