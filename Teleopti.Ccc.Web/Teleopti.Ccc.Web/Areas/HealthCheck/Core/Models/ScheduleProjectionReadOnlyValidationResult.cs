using System;

namespace Teleopti.Ccc.Web.Areas.HealthCheck.Core.Models
{
	public class ScheduleProjectionReadOnlyValidationResult
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public bool IsValid { get; set; }
	}
}