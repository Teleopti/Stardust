﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class MinWeeklyRestRule : INewBusinessRule
	{
		private bool _haltModify = true;
		private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
		private readonly IPersonWeekViolatingWeeklyRestSpecification _personWeekViolatingWeeklyRestSpecification;

		public MinWeeklyRestRule(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor,
			IPersonWeekViolatingWeeklyRestSpecification personWeekViolatingWeeklyRestSpecification)
		{
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
			_personWeekViolatingWeeklyRestSpecification = personWeekViolatingWeeklyRestSpecification;
			FriendlyName = string.Empty;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId =
				"Teleopti.Ccc.Domain.Scheduling.Rules.MinWeeklyRestRule.createResponse(Teleopti.Interfaces.Domain.IPerson,Teleopti.Interfaces.Domain.DateOnly,System.String,System.Type)"
			)]
		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			var responseList = new HashSet<IBusinessRuleResponse>();
			var personWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDays, true);

			foreach (PersonWeek personWeek in personWeeks)
			{
				var person = personWeek.Person;
				IScheduleRange currentSchedules = rangeClones[person];
				var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
				foreach (DateOnly day in personWeek.Week.DayCollection())
				{
					oldResponses.Remove(createResponse(person, day, "remove", typeof (MinWeeklyRestRule), Resources.BusinessRuleNoContractErrorMessage));
				}

				TimeSpan weeklyRest;
				if (!setWeeklyRest(out weeklyRest, personWeek))
				{
					// set errors on all days
					foreach (DateOnly dateOnly in personWeek.Week.DayCollection())
					{
						string message = string.Format(CultureInfo.CurrentCulture,
							Resources.BusinessRuleNoContractErrorMessage, person.Name,
							dateOnly.Date.ToShortDateString());
						IBusinessRuleResponse response = createResponse(person, dateOnly, message,
							typeof (MinWeeklyRestRule), Resources.BusinessRuleNoContractErrorMessage);
						if (!ForDelete)
							responseList.Add(response);
						oldResponses.Add(response);
					}
				}
				else
				{
					if (!_personWeekViolatingWeeklyRestSpecification.IsSatisfyBy(currentSchedules, personWeek.Week, weeklyRest))
					{
						string weeklyRestString = DateHelper.HourMinutesString(weeklyRest.TotalMinutes);
						string message = string.Format(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture,
							Resources.BusinessRuleWeeklyRestErrorMessage, weeklyRestString);
						foreach (DateOnly dateOnly in personWeek.Week.DayCollection())
						{
							IBusinessRuleResponse response = createResponse(person, dateOnly, message,
								typeof (MinWeeklyRestRule), Resources.BusinessRuleWeeklyRestFriendlyName);
							responseList.Add(response);
							oldResponses.Add(response);
						}
						// }
					}
				}
			}

			return responseList;
		}

		public string FriendlyName { get; }

		private static bool setWeeklyRest(out TimeSpan weeklyRest, PersonWeek personWeek)
		{
			var person = personWeek.Person;

			var period = person.PersonPeriods(personWeek.Week);
			if (period.Count == 0)
			{
				weeklyRest = TimeSpan.FromSeconds(0);
				return false;
			}
			weeklyRest = period[0].PersonContract.Contract.WorkTimeDirective.WeeklyRest;

			return true;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type, string friendlyName)
		{
			var dop = new DateOnlyPeriod(dateOnly, dateOnly);
			DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			IBusinessRuleResponse response = new BusinessRuleResponse(type, message, _haltModify, IsMandatory, period, person,
				dop, friendlyName)
			{Overridden = !_haltModify};
			return response;
		}
	}
}
