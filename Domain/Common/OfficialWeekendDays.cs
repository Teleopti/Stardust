﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class OfficialWeekendDays : IOfficialWeekendDays
    {
    	public IList<int> WeekendDayIndexesRelativeStartDayOfWeek()
    	{
    		return new[] {5, 6};
    	}
    }
}
