using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class WeekShiftCategoryLimitationRule : INewBusinessRule
	{
		private readonly IShiftCategoryLimitationChecker _limitationChecker;
		private readonly IVirtualSchedulePeriodExtractor _virtualSchedulePeriodExtractor;
		private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;

		public WeekShiftCategoryLimitationRule(IShiftCategoryLimitationChecker limitationChecker,
			IVirtualSchedulePeriodExtractor virtualSchedulePeriodExtractor,
			IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor)
		{
			_limitationChecker = limitationChecker;
			_virtualSchedulePeriodExtractor = virtualSchedulePeriodExtractor;
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
		}

		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => true;

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			var errorMessage = Resources.BusinessRuleShiftCategoryLimitationErrorMessage;
			var responseList = new HashSet<IBusinessRuleResponse>();

			var scheduleDaysList = scheduleDays.ToArray();
			var virtualSchedulePeriods =
				_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(scheduleDaysList);
			var personWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDaysList).ToArray();
			var schedulePeriods = virtualSchedulePeriods as IVirtualSchedulePeriod[] ?? virtualSchedulePeriods.ToArray();

			IPerson anyPerson = null;
			IList<IBusinessRuleResponse> oldResponses = null;
			var oldResponseCount = 0;
			if (schedulePeriods.Any())
			{
				anyPerson = schedulePeriods.First().Person;
				var currentSchedules = rangeClones[anyPerson];
				oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
				oldResponseCount = oldResponses.Count;
			}

			foreach (var schedulePeriod in schedulePeriods)
			{
				if (!schedulePeriod.IsValid) continue;
				var person = schedulePeriod.Person;
				var timeZone = person.PermissionInformation.DefaultTimeZone();
				foreach (var personWeek in personWeeks)
				{
					foreach (var day in personWeek.Week.DayCollection())
					{
						var period = day.ToDateTimePeriod(timeZone);
						for (var i = oldResponses.Count - 1; i >= 0; i--)
						{
							var response = oldResponses[i];
							if (response.TypeOfRule == typeof(WeekShiftCategoryLimitationRule) &&
								response.Period.Equals(period) && response.Person.Equals(anyPerson))
								oldResponses.RemoveAt(i);
						}
					}
				}
			}

			foreach (var schedulePeriod in virtualSchedulePeriods)
			{
				if (!schedulePeriod.IsValid) continue;

				var scheduleDateOnlyPeriod = schedulePeriod.DateOnlyPeriod;
				var person = schedulePeriod.Person;
				var timeZone = schedulePeriod.Person.PermissionInformation.DefaultTimeZone();
				foreach (var shiftCategoryLimitation in schedulePeriod.ShiftCategoryLimitationCollection())
				{
					if (!shiftCategoryLimitation.Weekly) continue;

					foreach (var personWeek in personWeeks)
					{
						// vi måste kanske gör ngt annat om en vecka ligger i 2 olika schemaperioder (kan ju ha helt olika regler)
						if (personWeek.Week.Intersection(scheduleDateOnlyPeriod) == null || !personWeek.Person.Equals(person))
						{
							continue;
						}

						var schedule = rangeClones[person];

						IList<DateOnly> datesWithCategory;
						if (!_limitationChecker.IsShiftCategoryOverWeekLimit(shiftCategoryLimitation, schedule,
							personWeek.Week, out datesWithCategory))
						{
							continue;
						}

						var message = string.Format(errorMessage, shiftCategoryLimitation.ShiftCategory.Description.Name);
						foreach (var dateOnly in datesWithCategory)
						{
							var dop = dateOnly.ToDateOnlyPeriod();
							var period = dop.ToDateTimePeriod(timeZone);

							if (!ForDelete)
								responseList.Add(createResponse(person, dop, period, message,
									typeof(WeekShiftCategoryLimitationRule)));
							oldResponses.Add(createResponse(person, dop, period, message,
								typeof(WeekShiftCategoryLimitationRule)));
						}
					}
				}
				var newResponseCount = responseList.Count;
				if (newResponseCount <= oldResponseCount)
					responseList = new HashSet<IBusinessRuleResponse>();
			}
			return responseList;
		}

		public string Description => Resources.DescriptionOfWeekShiftCategoryLimitationRule;

		private IBusinessRuleResponse createResponse(IPerson person, DateOnlyPeriod dop, DateTimePeriod period, string message,
			Type type)
		{
			var friendlyName = Resources.BusinessRuleShiftCategoryLimitationFriendlyName1;
			return new BusinessRuleResponse(type, message, HaltModify, IsMandatory, period, person, dop,
				friendlyName) {Overridden = !HaltModify};
		}
	}
}