using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class WeekShiftCategoryLimitationRule : INewBusinessRule
    {
        private readonly IShiftCategoryLimitationChecker _limitationChecker;
        private readonly IVirtualSchedulePeriodExtractor _virtualSchedulePeriodExtractor;
        private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
        private bool _haltModify = true;

        public WeekShiftCategoryLimitationRule(IShiftCategoryLimitationChecker limitationChecker,
            IVirtualSchedulePeriodExtractor virtualSchedulePeriodExtractor, IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor)
        {
            _limitationChecker = limitationChecker;
            _virtualSchedulePeriodExtractor = virtualSchedulePeriodExtractor;
            _weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
        }

        public string ErrorMessage { get { return ""; } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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

            var scheduleDaysList = new List<IScheduleDay>(scheduleDays);
            var virtualSchedulePeriods =
                _virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(scheduleDaysList);
            var personWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDaysList).ToList();
            var schedulePeriods = virtualSchedulePeriods as IVirtualSchedulePeriod[] ?? virtualSchedulePeriods.ToArray();
			
            IPerson anyPerson = null;
			IScheduleRange currentSchedules = null;
			IList<IBusinessRuleResponse> oldResponses = null;
	        int oldResponseCount = 0;
			if (schedulePeriods.Any())
	        {
		        anyPerson = schedulePeriods.First().Person;
		        currentSchedules = rangeClones[anyPerson];
		        oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
				oldResponseCount = oldResponses.Count();
	        }

            foreach (IVirtualSchedulePeriod schedulePeriod in schedulePeriods)
            {
                if (schedulePeriod.IsValid)
                {
                    foreach (PersonWeek personWeek in personWeeks)
                    {
                        foreach (DateOnly day in personWeek.Week.DayCollection())
                        {
                            var dop = new DateOnlyPeriod(day, day);
                            DateTimePeriod period = dop.ToDateTimePeriod(anyPerson.PermissionInformation.DefaultTimeZone());
                            for (int i = oldResponses.Count - 1; i >= 0; i--)
                            {
                                var response = oldResponses[i];
	                            if (response.TypeOfRule == typeof (WeekShiftCategoryLimitationRule) &&
	                                response.Period.Equals(period) && response.Person.Equals(anyPerson))
                                    oldResponses.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            foreach (IVirtualSchedulePeriod schedulePeriod in virtualSchedulePeriods)
            {
                if (schedulePeriod.IsValid)
                {
                    DateOnlyPeriod scheduleDateOnlyPeriod = schedulePeriod.DateOnlyPeriod;
                    var person = schedulePeriod.Person;
                    
                    foreach (var shiftCategoryLimitation in schedulePeriod.ShiftCategoryLimitationCollection())
                    {
                        if (shiftCategoryLimitation.Weekly)
                        {
                            foreach (PersonWeek personWeek in personWeeks)
                            {
                                // vi m�ste kanske g�r ngt annat om en vecka ligger i 2 olika schemaperioder (kan ju ha helt olika regler)
                                if (personWeek.Week.Intersection(scheduleDateOnlyPeriod) != null && personWeek.Person.Equals(person))
                                {
	                                foreach (var schedule in rangeClones.Values)
									{
										IList<DateOnly> datesWithCategory;
										if (_limitationChecker.IsShiftCategoryOverWeekLimit(shiftCategoryLimitation, schedule, personWeek.Week, out datesWithCategory))
										{
											string message = string.Format(TeleoptiPrincipal.Current.Regional.Culture,
											                               Resources.BusinessRuleShiftCategoryLimitationErrorMessage,
											                               shiftCategoryLimitation.ShiftCategory.Description.Name);
											foreach (DateOnly dateOnly in datesWithCategory)
											{
												if (!ForDelete)
													responseList.Add(createResponse(person, dateOnly, message, typeof(WeekShiftCategoryLimitationRule)));
												oldResponses.Add(createResponse(person, dateOnly, message, typeof(WeekShiftCategoryLimitationRule)));
											}
										}
									}
                                }
                            }
                        }
                    }
                    var newResponseCount = responseList.Count();
                    if (newResponseCount <= oldResponseCount)
                        responseList = new HashSet<IBusinessRuleResponse>();
                }
            }
         return responseList;
        }

        private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            IBusinessRuleResponse response = new BusinessRuleResponse(type, message, _haltModify, IsMandatory, period, person, dop) { Overridden = !_haltModify };
            return response;
        }
    }
}
