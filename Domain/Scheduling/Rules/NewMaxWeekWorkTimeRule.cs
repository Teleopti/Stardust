using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NewMaxWeekWorkTimeRule : INewBusinessRule
	{
		private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
		private readonly string _localizedMessage1;
		private readonly string _localizedMessage2;

		public NewMaxWeekWorkTimeRule(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor)
		{
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
			FriendlyName = Resources.BusinessRuleMaxWeekWorkTimeFriendlyName;
			Description = Resources.DescriptionOfNewMaxWeekWorkTimeRule;
			_localizedMessage1 = Resources.BusinessRuleMaxWeekWorkTimeErrorMessage;
			_localizedMessage2 = Resources.BusinessRuleNoContractErrorMessage;
		}

		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => true;

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
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
					oldResponses.Remove(createResponse(person, day, "remove", typeof(NewMaxWeekWorkTimeRule)));
				}
				double maxTimePerWeekMinutes;
				if (!setMaxTimePerWeekMinutes(out maxTimePerWeekMinutes, personWeek))
				{
					// set errors on all days
					foreach (var dateOnly in personWeek.Week.DayCollection())
					{
						var message = string.Format(CultureInfo.CurrentCulture, _localizedMessage2, person.Name,
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
					var message = string.Format(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture, _localizedMessage1,
						sumWorkTimeString, maxTimePerWeekString);
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

		public string FriendlyName { get; }
		public string Description { get; }

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
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var response = new BusinessRuleResponse(type, message, HaltModify, IsMandatory, period, person, dop, FriendlyName)
												 {Overridden = !HaltModify};
			return response;
		}
	}
}