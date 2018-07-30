using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule
{
	public class AbsenceReportInput
	{
		public DateOnly Date { get; set; }
		public Guid AbsenceId { get; set; }

	}
}