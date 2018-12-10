using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeAvailability : IPersistableScheduleData
	{
		bool NotAvailable { get; set; }
		TimeSpan? StartTime { get; }
		TimeSpan? EndTime { get; }
		bool IsDeleted { get; }
		DateOnly DateOfOvertime { get; }
	}
}
