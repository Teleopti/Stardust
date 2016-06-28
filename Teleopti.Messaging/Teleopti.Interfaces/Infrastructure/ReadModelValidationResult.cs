using System;

namespace Teleopti.Interfaces.Infrastructure
{
	public enum ValidateReadModelType
	{
		ScheduleProjectionReadOnly,
		PersonScheduleDay,
		ScheduleDay
	}

	public class ReadModelValidationResult
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public bool IsValid { get; set; }
		public ValidateReadModelType Type { get; set; }
	}
}