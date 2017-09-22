using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class AddOvertimeActivityForm
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public PersonDate[] PersonDates { get; set; }
		public Guid ActivityId { get; set; }
		public Guid MultiplicatorDefinitionSetId { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}