using System;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData {
	public class OvertimeRequestInput
	{
		public Guid? Id { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }
		public MultiplicatorDefinitionSet MultiplicatorDefinitionSet { get; set; }
		public DateTime StartTime { get; set; }
		public TimeSpan Duration { get; set; }
	}
}