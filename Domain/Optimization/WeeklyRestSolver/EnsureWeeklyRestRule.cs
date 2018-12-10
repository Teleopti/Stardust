using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;

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

	    private class WeeklyRestDetailForValidation
	    {
			public IPersonAssignment PersonAssignment { get; set; }
			public IScheduleDay ScheduleDay { get; set; }
	    }

        public  bool HasMinWeeklyRest(PersonWeek personWeek, IScheduleRange currentSchedules, TimeSpan weeklyRest)
        {
            //TODO rewrite this class in a better way
            var extendedWeek = personWeek.Week.Inflate(1);
            var schedules =
		        currentSchedules.ScheduledDayCollection(extendedWeek.Inflate(1))
			        .ToDictionary(k => k.DateOnlyAsPeriod.DateOnly,
				        s =>
					        new WeeklyRestDetailForValidation
					        {
						        PersonAssignment = s.PersonAssignment(),
						        ScheduleDay = s
					        });

	        var pAss = schedules.Where(s => s.Value.PersonAssignment != null && extendedWeek.Contains(s.Key)).ToList();
            if (pAss.Count == 0)
                return true;

            DateTime endOfPeriodBefore = TimeZoneHelper.ConvertToUtc(extendedWeek.StartDate.Date, personWeek.Person.PermissionInformation.DefaultTimeZone());

            var scheduleDayBefore1 = schedules[personWeek.Week.StartDate.AddDays(-1)].ScheduleDay;
            var scheduleDayBefore2 = schedules[personWeek.Week.StartDate.AddDays(-2)].ScheduleDay;
            var result = _dayOffMaxFlexCalculator.MaxFlex(scheduleDayBefore1, scheduleDayBefore2);
            if (result != null)
                endOfPeriodBefore = result.Value.EndDateTime;

            foreach (var ass in pAss)
            {
				if (ass.Value.ScheduleDay.IsFullDayAbsence()) continue;
				var proj = ass.Value.PersonAssignment.ProjectionService().CreateProjection();
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
            var endOfPeriodAfter = TimeZoneHelper.ConvertToUtc(extendedWeek.EndDate.AddDays(1).Date, personWeek.Person.PermissionInformation.DefaultTimeZone());
			var scheduleDayAfter1 = schedules[personWeek.Week.EndDate.AddDays(1)].ScheduleDay;
			var scheduleDayAfter2 = schedules[personWeek.Week.EndDate.AddDays(2)].ScheduleDay;
            result = _dayOffMaxFlexCalculator.MaxFlex(scheduleDayAfter1, scheduleDayAfter2);
            if (result != null)
                endOfPeriodAfter = result.Value.EndDateTime;

            if ((endOfPeriodAfter - endOfPeriodBefore) >= weeklyRest)
                return true;

            return false;
        }
    }
}