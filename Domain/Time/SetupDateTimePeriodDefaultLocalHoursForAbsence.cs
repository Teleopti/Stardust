using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodDefaultLocalHoursForAbsence: ISetupDateTimePeriod
    {
	    private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
	    private readonly DateTimePeriod _period;

        public SetupDateTimePeriodDefaultLocalHoursForAbsence(IScheduleDay scheduleDay, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
        {
	        _currentTeleoptiPrincipal = currentTeleoptiPrincipal;
	        _period = GetPeriodFromScheduleDays(scheduleDay);
        }

	    private DateTimePeriod GetPeriodFromScheduleDays(IScheduleDay scheduleDay)
        {
            var defaultPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(scheduleDay.Period.LocalStartDateTime.Add(TimeSpan.Zero), scheduleDay.Period.LocalStartDateTime.Add(TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59))),_currentTeleoptiPrincipal.Current().Regional.TimeZone);
            var setupDateTimePeriodToDefaultLocalHours =
                new SetupDateTimePeriodToDefaultLocalHours(
                    defaultPeriod,
                    null,
                    scheduleDay.TimeZone);

            return setupDateTimePeriodToDefaultLocalHours.Period;
        }

        public DateTimePeriod Period
        {
            get { return _period; }
        }
    }
}