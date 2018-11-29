using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule
{
	public class AbsenceReportInput
	{
		public DateOnly Date { get; set; }
		public Guid AbsenceId { get; set; }

	}
}