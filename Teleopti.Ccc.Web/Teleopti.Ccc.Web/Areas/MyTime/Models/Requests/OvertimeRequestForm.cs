using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class OvertimeRequestForm
	{
		public Guid? Id { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }
		public Guid MultiplicatorDefinitionSet { get; set; }
		public DateTimePeriodForm Period { get; set; }
	}
}