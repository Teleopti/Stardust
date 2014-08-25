using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class MinWeekWorkTimeRule : INewBusinessRule
    {
        private bool _haltModify = true;
        private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;

        public MinWeekWorkTimeRule(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor)
        {
            _weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
        }

        public string ErrorMessage
        {
            get { return ""; }
        }

        public bool IsMandatory
        {
            get { return false; }
        }

        public bool HaltModify
        {
            get { return _haltModify; }
            set { _haltModify = value; }
        }

        public bool ForDelete { get; set; }

        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();
            var personWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDays);

            foreach (var personWeek in personWeeks)
            {
                var person = personWeek.Person;
                var currentSchedules = rangeClones[person];
                var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
                foreach (var day in personWeek.Week.DayCollection())
                {
                    oldResponses.Remove(createResponse(person, day, "remove", typeof(MinWeekWorkTimeRule)));
                }

                double minTimePerWeekMinutes;

                if (!setMinTimePerWeekMinutes(out minTimePerWeekMinutes, personWeek)) continue;
                bool missingSchedule;
                var sumWorkTime = getSumWorkTime(out missingSchedule, personWeek, currentSchedules);

                if ((sumWorkTime >= minTimePerWeekMinutes) || missingSchedule) continue;
                var sumWorkTimeString = DateHelper.HourMinutesString(sumWorkTime);
                var minTimePerWeekString = DateHelper.HourMinutesString(minTimePerWeekMinutes);
                var message = string.Format(TeleoptiPrincipal.Current.Regional.Culture, UserTexts.Resources.BusinessRuleMinWeekWorktimeErrorMessage, sumWorkTimeString, minTimePerWeekString);
                foreach (var dateOnly in personWeek.Week.DayCollection())
                {
                    var response = createResponse(person, dateOnly, message, typeof(MinWeekWorkTimeRule));
                    if (!ForDelete)
                        responseList.Add(response);
                    oldResponses.Add(response);
                }
            }
            return responseList;
        }

        private static bool setMinTimePerWeekMinutes(out double minTimePerWeek, PersonWeek personWeek)
        {
            var person = personWeek.Person;
            var min = double.MaxValue;
            var haveFullTimePersonPeriod = false;

            foreach (var dateOnly in personWeek.Week.DayCollection())
            {
                var period = person.Period(dateOnly);
                if (period == null) continue;
                var tmpMin = period.PersonContract.Contract.WorkTimeDirective.MinTimePerWeek.TotalMinutes;
                if (tmpMin < min) min = tmpMin;
                if (!period.PersonContract.Contract.EmploymentType.Equals(EmploymentType.HourlyStaff)) haveFullTimePersonPeriod = true;
            }

            if (!haveFullTimePersonPeriod)
            {
                minTimePerWeek = 0;
                return false;
            }

            minTimePerWeek = min;
            return true;
        }

        private static double getSumWorkTime(out bool missingSchedule, PersonWeek personWeek, IScheduleRange currentSchedules)
        {
            double ctrTime = 0;
            var noSchedule = false;
            foreach (var schedule in currentSchedules.ScheduledDayCollection(personWeek.Week))
            {
                ctrTime += schedule.ProjectionService().CreateProjection().WorkTime().TotalMinutes;
                if (!schedule.IsScheduled()) noSchedule = true;
            }

            missingSchedule = noSchedule;
            return ctrTime;
        }

        private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            IBusinessRuleResponse response = new BusinessRuleResponse(type, message, _haltModify, IsMandatory, period, person, dop) { Overridden = !_haltModify };
            return response;
        }
    }
}
