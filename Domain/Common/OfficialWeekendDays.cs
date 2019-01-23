using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class OfficialWeekendDays : IOfficialWeekendDays
    {
    	public HashSet<int> WeekendDayIndexesRelativeStartDayOfWeek()
		{
			return new HashSet<int>(new[] { 5, 6 });
		}
    }
}
