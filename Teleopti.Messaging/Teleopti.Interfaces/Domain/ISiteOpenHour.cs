using System;

namespace Teleopti.Interfaces.Domain
{
	public interface ISiteOpenHour : IAggregateRoot
	{
		ISite Parent { get; set; }
		DayOfWeek WeekDay { get; set; }
		TimePeriod TimePeriod { get; set; }
		bool IsClosed { get; set; }
	}
}
