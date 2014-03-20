using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class MinWeeklyRestRule : INewBusinessRule
    {
        private bool _haltModify = true;
        private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
    	private readonly IWorkTimeStartEndExtractor _workTimeStartEndExtractor;
	    private readonly IDayOffMaxFlexCalculator _dayOffMaxFlexCalculator;

    	public MinWeeklyRestRule(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor, IWorkTimeStartEndExtractor workTimeStartEndExtractor, IDayOffMaxFlexCalculator dayOffMaxFlexCalculator)
		{
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
			_workTimeStartEndExtractor = workTimeStartEndExtractor;
    		_dayOffMaxFlexCalculator = dayOffMaxFlexCalculator;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Scheduling.Rules.MinWeeklyRestRule.createResponse(Teleopti.Interfaces.Domain.IPerson,Teleopti.Interfaces.Domain.DateOnly,System.String,System.Type)")]
		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new HashSet<IBusinessRuleResponse>();
            var personWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDays,true);
            
            foreach (PersonWeek personWeek in personWeeks)
            {
                var person = personWeek.Person;
                IScheduleRange currentSchedules = rangeClones [person];
                var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
                foreach (DateOnly day in personWeek.Week.DayCollection())
                {
                    oldResponses.Remove(createResponse(person, day, "remove", typeof(MinWeeklyRestRule)));
                }

                TimeSpan weeklyRest;
                if (!setWeeklyRest(out weeklyRest, personWeek))
                {
                    // set errors on all days
                    foreach (DateOnly dateOnly in personWeek.Week.DayCollection())
                    {
                        string message = string.Format(CultureInfo.CurrentCulture,
                                                 UserTexts.Resources.BusinessRuleNoContractErrorMessage, person.Name,
                                                 dateOnly.Date.ToShortDateString());
                        IBusinessRuleResponse response = createResponse(person, dateOnly, message,
                                                                        typeof(MinWeeklyRestRule));
                        if(!ForDelete)
                            responseList.Add(response);
                        oldResponses.Add(response);
                    }
                }
                else
                {
                    if (!hasMinWeeklyRest(personWeek, currentSchedules, weeklyRest))
                    {
                        string weeklyRestString = DateHelper.HourMinutesString(weeklyRest.TotalMinutes);
                        string message = string.Format(TeleoptiPrincipal.Current.Regional.Culture,
                                                UserTexts.Resources.BusinessRuleWeeklyRestErrorMessage, weeklyRestString);
                        foreach (DateOnly dateOnly in personWeek.Week.DayCollection())
                        {
                            IBusinessRuleResponse response = createResponse(person, dateOnly, message,
                                                                            typeof(MinWeeklyRestRule));
                            responseList.Add(response);
                            oldResponses.Add(response);
                        }
                    }
                }
            }

            return responseList;
        }

        private bool hasMinWeeklyRest(PersonWeek personWeek, IScheduleRange currentSchedules, TimeSpan weeklyRest)
        {
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

            DateTime endOfPeriodBefore = TimeZoneHelper.ConvertToUtc(extendedWeek.StartDate, personWeek.Person.PermissionInformation.DefaultTimeZone());

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
				if(nextStartDateTime != null)
				{
					if ((nextStartDateTime - endOfPeriodBefore) >= weeklyRest)
					{                   
                        // the majority must be in this week
                        if (endOfPeriodBefore.Add(TimeSpan.FromMinutes(weeklyRest.TotalMinutes / 2.0)) <= personWeek.Week.EndDate.AddDays(1) && nextStartDateTime.Value.Add(TimeSpan.FromMinutes(-weeklyRest.TotalMinutes / 2.0)) > personWeek.Week.StartDate)
					    return true;
					}
					var end = _workTimeStartEndExtractor.WorkTimeEnd(proj);
					if(end.HasValue)
						endOfPeriodBefore = end.Value;
				}
            }
            DateTime endOfPeriodAfter = TimeZoneHelper.ConvertToUtc(extendedWeek.EndDate.AddDays(1), personWeek.Person.PermissionInformation.DefaultTimeZone());


			var scheduleDayAfter1 = currentSchedules.ScheduledDay(personWeek.Week.EndDate.AddDays(1));
			var scheduleDayAfter2 = currentSchedules.ScheduledDay(personWeek.Week.EndDate.AddDays(2));
			result = _dayOffMaxFlexCalculator.MaxFlex(scheduleDayAfter1, scheduleDayAfter2);
			if (result != null)
				endOfPeriodAfter = result.Value.StartDateTime; 

            if ((endOfPeriodAfter - endOfPeriodBefore) >= weeklyRest)
                return true;

            return false;
        }

        private static bool setWeeklyRest(out TimeSpan weeklyRest, PersonWeek personWeek)
        {
            var person = personWeek.Person;
			// If the Person starts in the week we cant't find a person period
			// then we try with the last day
			var period = person.Period(personWeek.Week.StartDate) ?? person.Period(personWeek.Week.EndDate);
            if (period == null)
            {
                weeklyRest = TimeSpan.FromSeconds(0);
                return false;
            }
            weeklyRest = period.PersonContract.Contract.WorkTimeDirective.WeeklyRest;

            return true;
        }

        private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            IBusinessRuleResponse response = new BusinessRuleResponse(type, message, _haltModify, IsMandatory, period, person, dop)
                                                 {Overridden = !_haltModify};
            return response;
        }
    }
}
