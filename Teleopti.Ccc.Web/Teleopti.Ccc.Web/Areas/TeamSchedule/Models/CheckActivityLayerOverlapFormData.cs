using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class CheckActivityLayerOverlapFormData
	{
		public Guid[] PersonIds;
		public DateOnly Date { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}