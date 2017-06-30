using System;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData {
	public class OvertimeRequestInput
	{
		public Guid? Id { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }
		public MultiplicatorDefinitionSet MultiplicatorDefinitionSet { get; set; }
		public DateTimePeriodForm Period { get; set; }
	}
}