using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    /// <summary>
    /// Sets to 8-17 local on startdate
    /// </summary>
    public class SetupDateTimePeriodToDefaultLocalHours : ISetupDateTimePeriod
    {
	    public SetupDateTimePeriodToDefaultLocalHours(DateTimePeriod defaultLocal, IUserTimeZone userTimeZone, TimeZoneInfo info)
		{
			var timeZone = userTimeZone.TimeZone();
            Period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(defaultLocal.StartDateTimeLocal(timeZone), defaultLocal.EndDateTimeLocal(timeZone), info);
        }

        public DateTimePeriod Period { get;  }
    }
}