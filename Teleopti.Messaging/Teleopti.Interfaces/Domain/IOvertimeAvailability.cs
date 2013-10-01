using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IOvertimeAvailability : IPersistableScheduleData
	{
		bool NotAvailable { get; set; }
		TimeSpan? StartTime { get; }
		TimeSpan? EndTime { get; }
		bool IsDeleted { get; }
	}
}
