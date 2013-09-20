using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodDefaultLocalHoursForActivities : ISetupDateTimePeriod
    {
	    private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
	    private readonly DateTimePeriod _period;
        private DateTimePeriod _tp;

        public SetupDateTimePeriodDefaultLocalHoursForActivities(IScheduleDay scheduleDay, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
        {
	        _currentTeleoptiPrincipal = currentTeleoptiPrincipal;
	        HasDefaultValue = false;
	        _period = GetPeriodFromScheduleDays(scheduleDay);
        }

		public SetupDateTimePeriodDefaultLocalHoursForActivities(IScheduleDay scheduleDay, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, DateTimePeriod defaultPeriod)
        {
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_tp = defaultPeriod;
            HasDefaultValue = true;
            _period = GetPeriodFromScheduleDays(scheduleDay);
        }

        private DateTimePeriod GetPeriodFromScheduleDays(IScheduleDay scheduleDay)
        {
            var defaultPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(scheduleDay.Period.LocalStartDateTime.Add(TimeSpan.FromHours(8)), scheduleDay.Period.LocalStartDateTime.Add(TimeSpan.FromHours(17)), _currentTeleoptiPrincipal.Current().Regional.TimeZone);
            var dateTimePeriod = HasDefaultValue ? _tp : defaultPeriod;
            var setupDateTimePeriodToDefaultLocalHours =
                new SetupDateTimePeriodToDefaultLocalHours(
                    dateTimePeriod,
                    null,
                    scheduleDay.TimeZone);

            return setupDateTimePeriodToDefaultLocalHours.Period;
        }

        public DateTimePeriod Period
        {
            get { return _period; }
        }

        protected static bool HasDefaultValue
        {
            get;
            private set;
        }
    }
}
