using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class BackoutScheduleChangeFormData
	{
		public PersonDate[] PersonDates;			
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}