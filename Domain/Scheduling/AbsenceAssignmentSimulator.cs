using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class AbsenceAssignmentSimulator : IAbsenceAssignmentSimulator
    {
        private readonly IScheduleDictionary _scheduleDictionary;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

        public AbsenceAssignmentSimulator(IScheduleDictionary scheduleDictionary, IScheduleDayChangeCallback scheduleDayChangeCallback)
        {
            _scheduleDictionary = scheduleDictionary;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public IScheduleRange AssignAbsence(IAbsenceRequest absenceRequest)
        {
            IScheduleRange scheduleRange = _scheduleDictionary[absenceRequest.Person];
            DateOnlyPeriod dateOnlyPeriod = absenceRequest.Period.ToDateOnlyPeriod(scheduleRange.Person.PermissionInformation.DefaultTimeZone());

            var partList = new List<IScheduleDay>();
            foreach (var dateOnly in dateOnlyPeriod.DayCollection())
            {
                IScheduleDay part = scheduleRange.ScheduledDay(dateOnly);
                var intersectingPeriod = absenceRequest.Period.Intersection(part.Period);
                if (intersectingPeriod.HasValue)
                {
                    part.CreateAndAddAbsence(new AbsenceLayer(absenceRequest.Absence, intersectingPeriod.Value));
                    partList.Add(part);
                }
            }
            
            _scheduleDictionary.Modify(ScheduleModifier.Request, partList, NewBusinessRuleCollection.Minimum(), _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));

            return scheduleRange;
        }
    }
}