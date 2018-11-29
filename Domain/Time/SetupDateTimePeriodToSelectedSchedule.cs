using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodToSelectedSchedule : ISetupDateTimePeriod
    {
        private readonly ISetupDateTimePeriod _fallback;
        private readonly DateTimePeriod? _period;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public SetupDateTimePeriodToSelectedSchedule(IScheduleDay scheduleDay, ISetupDateTimePeriod fallback)
        {
            _fallback = fallback;
            var assignment = scheduleDay.PersonAssignment();
            if (assignment!=null && assignment.ShiftLayers.Any())
            {
                _period = assignment.Period;
            }
        }

        public DateTimePeriod Period
        {
            get
            {
                if (_period.HasValue) 
                    return _period.Value;
                return _fallback.Period;
            }
        }
    }
}