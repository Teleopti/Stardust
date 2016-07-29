using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class ScheduleValidationProvider:IScheduleValidationProvider
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonRepository _personRepository;

		public ScheduleValidationProvider(IScheduleStorage scheduleStorage,ICurrentScenario currentScenario,IPersonRepository personRepository)
		{
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_personRepository = personRepository;
		}


		public IList<BusinessRuleValidationResult> GetBusinessRuleValidationResults(FetchRuleValidationResultFormData input, BusinessRuleFlags ruleFlags)
		{
			var personIds = input.PersonIds;
			var people = _personRepository.FindPeople(personIds);
			var date = new DateOnly(input.Date);
			var dateOnlyPeriod = new DateOnlyPeriod(date, date);
			var scenario = _currentScenario.Current();

			var extendedPeriod = new DateOnlyPeriod(date, date);

			foreach (var person in people)
			{
				var period = DateHelper.GetWeekPeriod(date, person.FirstDayOfWeek);
				if (period.StartDate == date)
					period = new DateOnlyPeriod(period.StartDate.AddDays(-1), period.EndDate);
				if (period.EndDate == date)
					period = new DateOnlyPeriod(period.StartDate, period.EndDate.AddDays(1));
				if (extendedPeriod.Contains(period))
					continue;
				if (period.Contains(extendedPeriod))
				{
					extendedPeriod = period;
					continue;
				}
				if (extendedPeriod.StartDate > period.StartDate)
				{
					extendedPeriod = new DateOnlyPeriod(period.StartDate, extendedPeriod.EndDate);
					continue;
				}
				extendedPeriod = new DateOnlyPeriod(extendedPeriod.StartDate, period.EndDate);
			}

			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people,
				new ScheduleDictionaryLoadOptions(false, false),
				extendedPeriod,
				scenario);

			var scheduleDays = people.SelectMany(person =>
			{
				return schedules.SchedulesForPeriod(dateOnlyPeriod, person).Where(s => s.DateOnlyAsPeriod.DateOnly == date);
			});

			var rules = NewBusinessRuleCollection.New();

			if (ruleFlags.HasFlag(BusinessRuleFlags.NewNightlyRestRule))
			{
				rules.Add(new NewNightlyRestRule(new WorkTimeStartEndExtractor()));
			}
			if (ruleFlags.HasFlag(BusinessRuleFlags.MinWeekWorkTimeRule))
			{
				rules.Add(new MinWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor()));
			}
			if (ruleFlags.HasFlag(BusinessRuleFlags.NewMaxWeekWorkTimeRule))
			{
				rules.Add(new NewMaxWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor()));
			}

			var ruleResponse = rules.CheckRules(schedules, scheduleDays);

			var businessRuleValidationResults =
				ruleResponse.Where(
					r => dateOnlyPeriod.ToDateTimePeriod(r.Person.PermissionInformation.DefaultTimeZone()).Equals(r.Period)
					)
					.GroupBy(s => s.Person.Id.Value)
					.Select(x =>
					{
						return new BusinessRuleValidationResult
						{
							PersonId = x.Key,
							Warnings = x.Select(y => y.Message).ToList()
						};
					})
					.ToList();
			return businessRuleValidationResults;
		}
	}

	public interface IScheduleValidationProvider
	{
		IList<BusinessRuleValidationResult> GetBusinessRuleValidationResults(FetchRuleValidationResultFormData input, BusinessRuleFlags ruleFlags);
	}
}