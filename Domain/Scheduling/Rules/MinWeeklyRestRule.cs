using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class MinWeeklyRestRule : INewBusinessRule
	{
		private bool _haltModify = true;
		private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
		private readonly IPersonWeekVoilatingWeeklyRestSpecification _personWeekVoilatingWeeklyRestSpecification;

		public MinWeeklyRestRule(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor,
			IPersonWeekVoilatingWeeklyRestSpecification personWeekVoilatingWeeklyRestSpecification)
		{
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
			_personWeekVoilatingWeeklyRestSpecification = personWeekVoilatingWeeklyRestSpecification;
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
					oldResponses.Remove(createResponse(person, day, "remove", typeof (MinWeeklyRestRule)));
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
							typeof (MinWeeklyRestRule));
						if (!ForDelete)
							responseList.Add(response);
						oldResponses.Add(response);
					}
				}
				else
				{
					if (!_personWeekVoilatingWeeklyRestSpecification.IsSatisfyBy(currentSchedules, personWeek, weeklyRest))
					{
						string weeklyRestString = DateHelper.HourMinutesString(weeklyRest.TotalMinutes);
						string message = string.Format(TeleoptiPrincipal.Current.Regional.Culture,
							UserTexts.Resources.BusinessRuleWeeklyRestErrorMessage, weeklyRestString);
						foreach (DateOnly dateOnly in personWeek.Week.DayCollection())
						{
							IBusinessRuleResponse response = createResponse(person, dateOnly, message,
								typeof (MinWeeklyRestRule));
							responseList.Add(response);
							oldResponses.Add(response);
						}
						// }
					}
				}
			}

			return responseList;
		}

		private static bool setWeeklyRest(out TimeSpan weeklyRest, PersonWeek personWeek)
		{
			var person = personWeek.Person;

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
			IBusinessRuleResponse response = new BusinessRuleResponse(type, message, _haltModify, IsMandatory, period, person,
				dop)
			{Overridden = !_haltModify};
			return response;
		}
	}
}
