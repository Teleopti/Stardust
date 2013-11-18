using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IOvertimeAvailability : INonversionedPersistableScheduleData
	{
		bool NotAvailable { get; set; }
		TimeSpan? StartTime { get; }
		TimeSpan? EndTime { get; }
		bool IsDeleted { get; }
	}
}
