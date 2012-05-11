using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class AbsenceRequestForm
	{
		public string Subject { get; set; }
		public string Message { get; set; }
		public Guid AbsenceId { get; set; }
		public bool FullDay { get; set; }
		public DateTimePeriodForm Period { get; set; }
	}
}