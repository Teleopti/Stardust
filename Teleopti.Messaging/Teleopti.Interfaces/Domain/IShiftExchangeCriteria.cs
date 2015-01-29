using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
	public interface IShiftExchangeCriteria
	{
		bool IsValid(DateTimePeriod? targetShiftPeriod, bool targetDayOff = false);
		DateOnly ValidTo { get; set; }
		ShiftExchangeLookingForDay DayType { get; set; }		
	}
}
