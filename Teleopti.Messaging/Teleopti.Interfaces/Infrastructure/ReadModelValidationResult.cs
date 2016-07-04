using System;

namespace Teleopti.Interfaces.Infrastructure
{
	[Flags]public enum ValidateReadModelType
	{
		None,
		ScheduleProjectionReadOnly,
		PersonScheduleDay,
		ScheduleDay = 4
	}

	public class ReadModelValidationResult
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public bool IsValid { get; set; }
		public ValidateReadModelType Type { get; set; }
	}
}