using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class GetSchedulesForPeopleFormData
	{
		public List<Guid> PersonIds { get; set; }
		public DateTime Date { get; set; }
	}
}