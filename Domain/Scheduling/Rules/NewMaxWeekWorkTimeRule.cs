﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class NewMaxWeekWorkTimeRule : INewBusinessRule
    {
        private bool _haltModify = true;
        private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;

        public NewMaxWeekWorkTimeRule(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor)
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

        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();
            var personWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDays);

            foreach (PersonWeek personWeek in personWeeks)
            {
                var person = personWeek.Person;
                IScheduleRange currentSchedules = rangeClones[person];
                var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
                foreach (DateOnly day in personWeek.Week.DayCollection())
                {
                    oldResponses.Remove(createResponse(person, day, "remove", typeof(NewMaxWeekWorkTimeRule)));
                }
                double maxTimePerWeekMinutes;
                if (!setMaxTimePerWeekMinutes(out maxTimePerWeekMinutes, personWeek))
                {
                    // set errors on all days
                    foreach (DateOnly dateOnly in personWeek.Week.DayCollection())
                    {
                        string message = string.Format(CultureInfo.CurrentCulture,
                                                 UserTexts.Resources.BusinessRuleNoContractErrorMessage, person.Name,
                                                 dateOnly.Date.ToShortDateString());
                        IBusinessRuleResponse response = createResponse(person, dateOnly, message,
                                                                        typeof(NewMaxWeekWorkTimeRule));
                        if(!ForDelete)
                            responseList.Add(response);
                        oldResponses.Add(response);
                    }
                }
                else
                {
                    double sumWorkTime = getSumWorkTime(personWeek, currentSchedules);

                    if (maxTimePerWeekMinutes < sumWorkTime)
                    {
                        string sumWorkTimeString = DateHelper.HourMinutesString(sumWorkTime);
                        string maxTimePerWeekString = DateHelper.HourMinutesString(maxTimePerWeekMinutes);
                        string message = string.Format(TeleoptiPrincipal.Current.Regional.Culture,
                                                UserTexts.Resources.BusinessRuleMaxWeekWorkTimeErrorMessage,
                                                sumWorkTimeString,
                                                maxTimePerWeekString);
                        foreach (DateOnly dateOnly in personWeek.Week.DayCollection())
                        {
                            IBusinessRuleResponse response = createResponse(person, dateOnly, message,
                                                                            typeof(NewMaxWeekWorkTimeRule));
                            if (!ForDelete)
                                responseList.Add(response);
                            oldResponses.Add(response);
                        }
                    }
                }
            }
            return responseList;
        }

        private static bool setMaxTimePerWeekMinutes(out double maxTimePerWeek, PersonWeek personWeek)
        {
            var person = personWeek.Person;
			// If the Person starts in the week we cant't find a person period
			// then we try with the last day
            var period = person.Period(personWeek.Week.StartDate) ?? person.Period(personWeek.Week.EndDate);
			
        	if (period == null)
            {
                maxTimePerWeek = 0;
                return false;
            }
            maxTimePerWeek = period.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek.TotalMinutes;

            return true;
        }

        private static double getSumWorkTime(PersonWeek personWeek, IScheduleRange currentSchedules)
        {
            double ctrTime = 0;
            foreach (var day in personWeek.Week.DayCollection())
            {
                var schedule = currentSchedules.ScheduledDay(day);
                ctrTime += schedule.ProjectionService().CreateProjection().WorkTime().TotalMinutes;
            }

            return ctrTime;
        }
        private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
            IBusinessRuleResponse response = new BusinessRuleResponse(type, message, _haltModify, IsMandatory, period, person, dateOnlyPeriod)
                                                 {Overridden = !_haltModify};
            return response;
        }
    }
}
