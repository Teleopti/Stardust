using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public interface IBrokenWeekOutsideSelectionSpecification
    {
        bool IsSatisfy(PersonWeek personWeek, IList<PersonWeek> selctedPersonWeeks, Dictionary<PersonWeek, TimeSpan> weeklyRestInPersonWeek, IScheduleRange personScheduleRange);
    }

    public class BrokenWeekOutsideSelectionSpecification : IBrokenWeekOutsideSelectionSpecification
    {
        private readonly IEnsureWeeklyRestRule  _ensureWeeklyRestRule;

        public BrokenWeekOutsideSelectionSpecification(IEnsureWeeklyRestRule ensureWeeklyRestRule)
        {
            _ensureWeeklyRestRule = ensureWeeklyRestRule;
        }

        public  bool IsSatisfy(PersonWeek personWeek, IList<PersonWeek> selctedPersonWeeks, Dictionary<PersonWeek, TimeSpan> weeklyRestInPersonWeek, IScheduleRange personScheduleRange)
        {
            //check week before
            var dateInPreviousWeek = personWeek.Week.StartDate.AddDays(-1);
            var dateInNextWeek = personWeek.Week.EndDate.AddDays(1);
            var foundProblemInWeek = hasMinWeeklyRestProblem(selctedPersonWeeks, dateInPreviousWeek, personScheduleRange,weeklyRestInPersonWeek, personWeek);
            if (!foundProblemInWeek )
            {
                foundProblemInWeek = hasMinWeeklyRestProblem(selctedPersonWeeks, dateInNextWeek, personScheduleRange, weeklyRestInPersonWeek, personWeek);
            }
            return foundProblemInWeek;
        }

        private bool hasMinWeeklyRestProblem(IList<PersonWeek> selctedPersonWeeks, DateOnly date, IScheduleRange personScheduleRange, Dictionary<PersonWeek, TimeSpan> weeklyRestInPersonWeek, PersonWeek personWeek)
        {
            var previousPersonWeek = selctedPersonWeeks.FirstOrDefault(s => s.Week.Contains(date));
            if (previousPersonWeek == null) return false;
            if (!_ensureWeeklyRestRule.HasMinWeeklyRest(previousPersonWeek, personScheduleRange, weeklyRestInPersonWeek[personWeek])) return true;
            return false;
        }
    }
}