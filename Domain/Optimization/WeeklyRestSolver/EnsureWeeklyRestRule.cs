using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public interface IEnsureWeeklyRestRule
    {
        bool HasMinWeeklyRest(PersonWeek personWeek, IScheduleRange currentSchedules, TimeSpan weeklyRest);
    }

    public class EnsureWeeklyRestRule : IEnsureWeeklyRestRule
    {
        private readonly IWorkTimeStartEndExtractor _workTimeStartEndExtractor;
        private readonly IDayOffMaxFlexCalculator _dayOffMaxFlexCalculator;

        public EnsureWeeklyRestRule( IWorkTimeStartEndExtractor workTimeStartEndExtractor, IDayOffMaxFlexCalculator dayOffMaxFlexCalculator)
        {
            
            _workTimeStartEndExtractor = workTimeStartEndExtractor;
            _dayOffMaxFlexCalculator = dayOffMaxFlexCalculator;
        }

        public  bool HasMinWeeklyRest(PersonWeek personWeek, IScheduleRange currentSchedules, TimeSpan weeklyRest)
        {
            //TODO rewrite this class in a better way
            var extendedWeek = new DateOnlyPeriod(personWeek.Week.StartDate.AddDays(-1),
                personWeek.Week.EndDate.AddDays(1));
            var pAss = new List<IPersonAssignment>();
            foreach (var schedule in currentSchedules.ScheduledDayCollection(extendedWeek))
            {
                var ass = schedule.PersonAssignment();
                if (ass != null)
                {
                    pAss.Add(ass);
                }
            }
            if (pAss.Count == 0)
                return true;

            DateTime endOfPeriodBefore = TimeZoneHelper.ConvertToUtc(extendedWeek.StartDate.Date, personWeek.Person.PermissionInformation.DefaultTimeZone());

            var scheduleDayBefore1 = currentSchedules.ScheduledDay(personWeek.Week.StartDate.AddDays(-1));
            var scheduleDayBefore2 = currentSchedules.ScheduledDay(personWeek.Week.StartDate.AddDays(-2));
            var result = _dayOffMaxFlexCalculator.MaxFlex(scheduleDayBefore1, scheduleDayBefore2);
            if (result != null)
                endOfPeriodBefore = result.Value.EndDateTime;

            foreach (IPersonAssignment ass in pAss)
            {
                var proj = ass.ProjectionService().CreateProjection();
                var nextStartDateTime =
                    _workTimeStartEndExtractor.WorkTimeStart(proj);
                if (nextStartDateTime != null)
                {
                    if ((nextStartDateTime - endOfPeriodBefore) >= weeklyRest)
                    {
                        // the majority must be in this week
                        if (endOfPeriodBefore.Date.Add(TimeSpan.FromMinutes(weeklyRest.TotalMinutes / 2.0)) <= personWeek.Week.EndDate.Date.AddDays(1) && nextStartDateTime.Value.Add(TimeSpan.FromMinutes(-weeklyRest.TotalMinutes / 2.0)) > personWeek.Week.StartDate.Date)
                            return true;
                    }
                    var end = _workTimeStartEndExtractor.WorkTimeEnd(proj);
                    if (end.HasValue)
                        endOfPeriodBefore = end.Value;
                }
            }
            DateTime endOfPeriodAfter = TimeZoneHelper.ConvertToUtc(extendedWeek.EndDate.AddDays(1).Date, personWeek.Person.PermissionInformation.DefaultTimeZone());


            var scheduleDayAfter1 = currentSchedules.ScheduledDay(personWeek.Week.EndDate.AddDays(1));
            var scheduleDayAfter2 = currentSchedules.ScheduledDay(personWeek.Week.EndDate.AddDays(2));
            result = _dayOffMaxFlexCalculator.MaxFlex(scheduleDayAfter1, scheduleDayAfter2);
            if (result != null)
                endOfPeriodAfter = result.Value.StartDateTime;

            if ((endOfPeriodAfter - endOfPeriodBefore) >= weeklyRest)
                return true;

            return false;
        }
    }
}