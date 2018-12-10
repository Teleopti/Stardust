using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodToSchedulesIfTheyExist : ISetupDateTimePeriod
    {
        private readonly IList<IScheduleDay> _scheduleDays;
        private readonly ISetupDateTimePeriod _fallback;

        public SetupDateTimePeriodToSchedulesIfTheyExist(IList<IScheduleDay> scheduleDays, ISetupDateTimePeriod fallback)
        {
            _fallback = fallback;
            _scheduleDays = scheduleDays;
        }

        public DateTimePeriod Period
        {
            get
            {
                if (new SelectedSchedulesAreEmpty().IsSatisfiedBy(_scheduleDays))  
                    return _fallback.Period;
                return new SetupDateTimePeriodToSelectedSchedules(_scheduleDays).Period;
            }
        }
    }
}