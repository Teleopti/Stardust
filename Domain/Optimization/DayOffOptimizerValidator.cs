using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DayOffOptimizerValidator
    {
        private readonly INewDayOffRule _dayOffRule;

        public DayOffOptimizerValidator(INewDayOffRule dayOffRule)
        {
            _dayOffRule = dayOffRule;
        }

        public bool Validate(DateOnly dateOnly, IScheduleMatrixPro scheduleMatrixPro)
        {
            var dictionary = new Dictionary<IPerson, IScheduleRange> { { scheduleMatrixPro.Person, scheduleMatrixPro.ActiveScheduleRange } };
            var scheduleDayPro = scheduleMatrixPro.GetScheduleDayByKey(dateOnly);
            var scheduleDays = new List<IScheduleDay> { scheduleDayPro.DaySchedulePart() };
            
            return !_dayOffRule.Validate(dictionary, scheduleDays).Any();
        }
    }
}
