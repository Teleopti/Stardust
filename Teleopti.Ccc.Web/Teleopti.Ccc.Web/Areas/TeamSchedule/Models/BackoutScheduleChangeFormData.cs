using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class BackoutScheduleChangeFormData
	{
		public Guid[] PersonIds;	
		public DateOnly Date { get; set; }	
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}