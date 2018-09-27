using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    /// <summary>
    /// Sets to 8-17 local on startdate
    /// </summary>
    public class SetupDateTimePeriodToDefaultLocalHours : ISetupDateTimePeriod
    {
	    public SetupDateTimePeriodToDefaultLocalHours(DateTimePeriod defaultLocal, TimeZoneInfo info)
        {
            Period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(defaultLocal.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone), defaultLocal.EndDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone), info);
        }

        public DateTimePeriod Period { get;  }
    }
}