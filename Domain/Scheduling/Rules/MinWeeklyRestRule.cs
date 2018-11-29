using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class MinWeeklyRestRule : INewBusinessRule
	{
		private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
		private readonly IPersonWeekViolatingWeeklyRestSpecification _personWeekViolatingWeeklyRestSpecification;

		public MinWeeklyRestRule(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor,
			IPersonWeekViolatingWeeklyRestSpecification personWeekViolatingWeeklyRestSpecification)
		{
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
			_personWeekViolatingWeeklyRestSpecification = personWeekViolatingWeeklyRestSpecification;
		}

		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => true;

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			var noContractErrorMessage = Resources.BusinessRuleNoContractErrorMessage;
			var weeklyRestErrorMessage = Resources.BusinessRuleWeeklyRestErrorMessage;
			var weeklyRestFriendlyName = Resources.BusinessRuleWeeklyRestFriendlyName;

			var responseList = new HashSet<IBusinessRuleResponse>();
			var personWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDays, true);

			foreach (var personWeek in personWeeks)
			{
				var person = personWeek.Person;
				var currentSchedules = rangeClones[person];
				var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
				foreach (var day in personWeek.Week.DayCollection())
				{
					oldResponses.Remove(createResponse(person, day, "remove", typeof(MinWeeklyRestRule),
						noContractErrorMessage));
				}

				TimeSpan weeklyRest;
				if (!setWeeklyRest(out weeklyRest, personWeek))
				{
					// set errors on all days
					foreach (var dateOnly in personWeek.Week.DayCollection())
					{
						var message = string.Format(noContractErrorMessage, person.Name,
							dateOnly.Date.ToShortDateString());
						var response = createResponse(person, dateOnly, message,
							typeof(MinWeeklyRestRule), noContractErrorMessage);
						if (!ForDelete)
							responseList.Add(response);
						oldResponses.Add(response);
					}
				}
				else
				{
					if (_personWeekViolatingWeeklyRestSpecification.IsSatisfyBy(currentSchedules, personWeek.Week, weeklyRest))
						continue;

					var weeklyRestString = DateHelper.HourMinutesString(weeklyRest.TotalMinutes);
					var message = string.Format(weeklyRestErrorMessage, weeklyRestString);
					foreach (var dateOnly in personWeek.Week.DayCollection())
					{
						var response = createResponse(person, dateOnly, message,
							typeof(MinWeeklyRestRule), weeklyRestFriendlyName);
						responseList.Add(response);
						oldResponses.Add(response);
					}
				}
			}

			return responseList;
		}

		public string Description => Resources.DescriptionOfMinWeeklyRestRule;

		private static bool setWeeklyRest(out TimeSpan weeklyRest, PersonWeek personWeek)
		{
			var person = personWeek.Person;

			var period = person.PersonPeriods(personWeek.Week);
			if (period.Count == 0)
			{
				weeklyRest = TimeSpan.Zero;
				return false;
			}
			weeklyRest = period[0].PersonContract.Contract.WorkTimeDirective.WeeklyRest;

			return true;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type,
			string friendlyName)
		{
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var response = new BusinessRuleResponse(type, message, HaltModify, IsMandatory, period, person, dop, friendlyName)
			{
				Overridden = !HaltModify
			};
			return response;
		}
	}
}