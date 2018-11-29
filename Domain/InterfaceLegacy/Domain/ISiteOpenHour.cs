using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISiteOpenHour : IAggregateRoot
	{
		ISite Parent { get; set; }
		DayOfWeek WeekDay { get; set; }
		TimePeriod TimePeriod { get; set; }
		bool IsClosed { get; set; }
	}
}
