using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DayOffOptimizerValidator : IDayOffOptimizerValidator
    {
        private readonly INewDayOffRule _dayOffRule;

        public DayOffOptimizerValidator(INewDayOffRule dayOffRule)
        {
            _dayOffRule = dayOffRule;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public bool Validate(DateOnly dateOnly, IScheduleMatrixPro scheduleMatrixPro)
        {
            var dictionary = new Dictionary<IPerson, IScheduleRange> { { scheduleMatrixPro.Person, scheduleMatrixPro.ActiveScheduleRange } };
            var scheduleDayPro = scheduleMatrixPro.GetScheduleDayByKey(dateOnly);
            var scheduleDays = new List<IScheduleDay> { scheduleDayPro.DaySchedulePart() };
            
            return !_dayOffRule.Validate(dictionary, scheduleDays).Any();
        }
    }
}
