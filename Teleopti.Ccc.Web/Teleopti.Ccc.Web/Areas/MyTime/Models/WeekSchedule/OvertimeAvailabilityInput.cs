using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule
{
	public class OvertimeAvailabilityInput
	{
		public DateOnly Date { get; set; }
		public TimeOfDay StartTime { get; set; }
		public TimeOfDay EndTime { get; set; }
	}
}