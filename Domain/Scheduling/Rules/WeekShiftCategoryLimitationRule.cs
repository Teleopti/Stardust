using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
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

            foreach (IVirtualSchedulePeriod schedulePeriod in virtualSchedulePeriods)
            {
                if (schedulePeriod.IsValid)
				{
                    DateOnlyPeriod scheduleDateOnlyPeriod = schedulePeriod.DateOnlyPeriod;
                    var person = schedulePeriod.Person;
					var timeZone = person.PermissionInformation.DefaultTimeZone();
					var currentSchedules = rangeClones[person];
                    var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
                    foreach (PersonWeek personWeek in personWeeks)
                    {
                        foreach (DateOnly day in personWeek.Week.DayCollection())
                        {
							var period = new DateOnlyPeriod(day, day).ToDateTimePeriod(timeZone);
                            for (int i = oldResponses.Count - 1; i >= 0; i--)
                            {
                                var response = oldResponses[i];
                                if (response.TypeOfRule == typeof(WeekShiftCategoryLimitationRule) && response.Period.Equals(period) && response.Person.Equals(person))
                                    oldResponses.RemoveAt(i);
                            }
                        }
                    }
                    
                    foreach (var shiftCategoryLimitation in schedulePeriod.ShiftCategoryLimitationCollection())
                    {
                        if (shiftCategoryLimitation.Weekly)
                        {
                            foreach (PersonWeek personWeek in personWeeks)
                            {
                                //foreach (DateOnly day in personWeek.Week.DayCollection())
                                //{
                                //    oldResponses.Remove(createResponse(person, day, "remove", typeof(WeekShiftCategoryLimitationRule)));
                                //}

                                // vi måste kanske gör ngt annat om en vecka ligger i 2 olika schemaperioder (kan ju ha helt olika regler)
                                if (personWeek.Week.Intersection(scheduleDateOnlyPeriod) != null && personWeek.Person.Equals(person))
                                {
                                    IList<DateOnly> datesWithCategory;
                                    if (_limitationChecker.IsShiftCategoryOverWeekLimit(shiftCategoryLimitation, currentSchedules, personWeek.Week, out datesWithCategory))
                                    {
                                        string message = string.Format(TeleoptiPrincipal.Current.Regional.Culture,
                                                            UserTexts.Resources.BusinessRuleShiftCategoryLimitationErrorMessage,
                                                            shiftCategoryLimitation.ShiftCategory.Description.Name);
                                        foreach (DateOnly dateOnly in datesWithCategory)
                                        {
											var dop = new DateOnlyPeriod(dateOnly, dateOnly);
											DateTimePeriod period = dop.ToDateTimePeriod(timeZone);

                                            if (!ForDelete)
                                                responseList.Add(createResponse(person, dop, period, message, typeof(WeekShiftCategoryLimitationRule)));
                                            oldResponses.Add(createResponse(person, dop, period, message, typeof(WeekShiftCategoryLimitationRule)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return responseList;
        }

        private IBusinessRuleResponse createResponse(IPerson person, DateOnlyPeriod dop, DateTimePeriod period, string message, Type type)
        {
            IBusinessRuleResponse response = new BusinessRuleResponse(type, message, _haltModify, IsMandatory, period, person, dop) { Overridden = !_haltModify };
            return response;
        }
    }
}
