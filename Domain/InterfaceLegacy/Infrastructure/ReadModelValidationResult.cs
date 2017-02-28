using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	[Flags]public enum ValidateReadModelType
	{
		ScheduleProjectionReadOnly = 1,
		PersonScheduleDay = 2,
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