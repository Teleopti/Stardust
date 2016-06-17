using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ScheduleProjectionReadOnlyValidationResult
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public bool IsValid { get; set; }
	}
}