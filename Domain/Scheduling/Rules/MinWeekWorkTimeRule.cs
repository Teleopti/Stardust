using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class MinWeekWorkTimeRule : INewBusinessRule
	{
		private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
		private readonly string _businessRuleMinWeekWorktimeErrorMessage;

		public MinWeekWorkTimeRule(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor)
		{
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
			FriendlyName = Resources.BusinessRuleMinWeekWorktimeFriendlyName;
			Description = Resources.DescriptionOfMinWeekWorkTimeRule;
			_businessRuleMinWeekWorktimeErrorMessage = Resources.BusinessRuleMinWeekWorktimeErrorMessage;
		}

		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => true;

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,IEnumerable<IScheduleDay> scheduleDays)
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
					oldResponses.Remove(createResponse(person, day, "remove", typeof(MinWeekWorkTimeRule)));
				}
				TimeSpan minTimePerWeek;
				if (!setMinTimePerWeekMinutes(out minTimePerWeek, personWeek))
					continue;

				if(minTimePerWeek == TimeSpan.Zero)
					continue;

				bool missingSchedule;
				var sumWorkTime = getSumContractTime(out missingSchedule, personWeek, currentSchedules, minTimePerWeek);
				if ((sumWorkTime >= minTimePerWeek.TotalMinutes) || missingSchedule)
					continue;

				var sumWorkTimeString = DateHelper.HourMinutesString(sumWorkTime);
				var minTimePerWeekString = DateHelper.HourMinutesString(minTimePerWeek.TotalMinutes);
				var message = string.Format(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture, _businessRuleMinWeekWorktimeErrorMessage, sumWorkTimeString, minTimePerWeekString);
				foreach (var dateOnly in personWeek.Week.DayCollection())
				{
					var response = createResponse(person, dateOnly, message, typeof(MinWeekWorkTimeRule));
					if (!ForDelete)
						responseList.Add(response);
					oldResponses.Add(response);
				}
			}
			return responseList;
		}

		public string FriendlyName { get; }
		public string Description { get; }

		private static bool setMinTimePerWeekMinutes(out TimeSpan minTimePerWeek, PersonWeek personWeek)
		{
			var person = personWeek.Person;
			var min = double.MaxValue;
			var haveFullTimePersonPeriod = false;

			foreach (var period in person.PersonPeriods(personWeek.Week))
			{
				var tmpMin = period.PersonContract.Contract.WorkTimeDirective.MinTimePerWeek.TotalMinutes;
				if (tmpMin < min)
					min = tmpMin;
				if (!period.PersonContract.Contract.EmploymentType.Equals(EmploymentType.HourlyStaff))
					haveFullTimePersonPeriod = true;
			}

			if (!haveFullTimePersonPeriod)
			{
				minTimePerWeek = TimeSpan.Zero;
				return false;
			}

			minTimePerWeek = TimeSpan.FromMinutes(min);
			return true;
		}

		private static double getSumContractTime(out bool missingSchedule, PersonWeek personWeek, IScheduleRange currentSchedules, TimeSpan limit)
		{
			double ctrTime = 0;
			var noSchedule = false;
			foreach (var schedule in currentSchedules.ScheduledDayCollection(personWeek.Week))
			{
				ctrTime += schedule.ProjectionService().CreateProjection().ContractTime().TotalMinutes;
				if(ctrTime >= limit.TotalMinutes)
					break;

				if (!schedule.IsScheduled())
				{
					noSchedule = true;
					break;
				}
			}

			missingSchedule = noSchedule;
			return ctrTime;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
		{
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			IBusinessRuleResponse response = new BusinessRuleResponse(type, message, HaltModify, IsMandatory, period, person, dop, FriendlyName) { Overridden = !HaltModify };
			return response;
		}
	}
}