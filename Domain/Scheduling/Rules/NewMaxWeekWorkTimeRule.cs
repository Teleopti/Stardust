using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NewMaxWeekWorkTimeRule : INewBusinessRule
	{
		private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;

		public NewMaxWeekWorkTimeRule(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor)
		{
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
		}

		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => true;

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			var currentUiCulture = Thread.CurrentThread.CurrentUICulture;
			var responseList = new HashSet<IBusinessRuleResponse>();
			var personWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDays);

			foreach (var personWeek in personWeeks)
			{
				var person = personWeek.Person;
				var currentSchedules = rangeClones[person];
				var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
				foreach (var day in personWeek.Week.DayCollection())
				{
					oldResponses.Remove(createResponse(person, day, "remove", typeof(NewMaxWeekWorkTimeRule)));
				}
				double maxTimePerWeekMinutes;

				var maxWeekWorkTimeErrorMessage = Resources.BusinessRuleMaxWeekWorkTimeErrorMessage;
				var noContractErrorMessage = Resources.BusinessRuleNoContractErrorMessage;
				if (!setMaxTimePerWeekMinutes(out maxTimePerWeekMinutes, personWeek))
				{
					// set errors on all days
					foreach (var dateOnly in personWeek.Week.DayCollection())
					{
						var message = string.Format(currentUiCulture, noContractErrorMessage, person.Name,
							dateOnly.Date.ToShortDateString());
						var response = createResponse(person, dateOnly, message, typeof(NewMaxWeekWorkTimeRule));
						if (!ForDelete)
							responseList.Add(response);
						oldResponses.Add(response);
					}
				}
				else
				{
					var sumWorkTime = getSumWorkTime(personWeek, currentSchedules);

					if (!(maxTimePerWeekMinutes < sumWorkTime)) continue;

					var sumWorkTimeString = DateHelper.HourMinutesString(sumWorkTime);
					var maxTimePerWeekString = DateHelper.HourMinutesString(maxTimePerWeekMinutes);
					var message = string.Format(maxWeekWorkTimeErrorMessage, sumWorkTimeString, maxTimePerWeekString);
					foreach (var dateOnly in personWeek.Week.DayCollection())
					{
						var response = createResponse(person, dateOnly, message,
							typeof(NewMaxWeekWorkTimeRule));
						if (!ForDelete)
							responseList.Add(response);
						oldResponses.Add(response);
					}
				}
			}
			return responseList;
		}

		public string Description => Resources.DescriptionOfNewMaxWeekWorkTimeRule;

		private static bool setMaxTimePerWeekMinutes(out double maxTimePerWeek, PersonWeek personWeek)
		{
			var person = personWeek.Person;
			var max = double.MinValue;
			var noPeriod = true;

			foreach (var period in person.PersonPeriods(personWeek.Week))
			{
				noPeriod = false;
				var tmpMax = period.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek.TotalMinutes;
				if (tmpMax > max) max = tmpMax;
			}

			if (noPeriod)
			{
				maxTimePerWeek = 0;
				return false;
			}

			maxTimePerWeek = max;

			return true;
		}

		private static double getSumWorkTime(PersonWeek personWeek, IScheduleRange currentSchedules)
		{
			return currentSchedules.ScheduledDayCollection(personWeek.Week)
					.Sum(schedule => schedule.ProjectionService().CreateProjection().WorkTime().TotalMinutes);
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
		{
			var friendlyName = Resources.BusinessRuleMaxWeekWorkTimeFriendlyName;
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var response = new BusinessRuleResponse(type, message, HaltModify, IsMandatory, period, person, dop,
				friendlyName) {Overridden = !HaltModify};
			return response;
		}
	}
}