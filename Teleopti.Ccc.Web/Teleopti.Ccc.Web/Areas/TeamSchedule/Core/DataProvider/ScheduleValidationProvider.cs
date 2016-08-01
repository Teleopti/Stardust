using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;
using static Teleopti.Interfaces.Domain.DateHelper;

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
			var inflatedPeriod = dateOnlyPeriod.Inflate(1);

			var personPeriods = people.Select(person => GetWeekPeriod(date,person.FirstDayOfWeek)).ToList();
			personPeriods.Add(inflatedPeriod); 
			
			var extendedPeriod = new DateOnlyPeriod( personPeriods.Min(p => p.StartDate), personPeriods.Max(p => p.EndDate));			

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

			var ruleResponse = rules.CheckRules(schedules, scheduleDays).ToList();

			if (ruleFlags.HasFlag(BusinessRuleFlags.NewDayOffRule))
			{
				ruleResponse.AddRange(checkDayOff(date, people));
			}

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

		private IEnumerable<IBusinessRuleResponse> checkDayOff(DateOnly date, IEnumerable<IPerson> people )
		{
			var dateOnlyPeriod = new DateOnlyPeriod(date,date);
			var scenario = _currentScenario.Current();
			var inflatedPeriod = dateOnlyPeriod.Inflate(1);

			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people,
				new ScheduleDictionaryLoadOptions(false,false),
				inflatedPeriod,
				scenario);

			var scheduleDays = people.SelectMany(person =>
			{
				return schedules.SchedulesForPeriod(inflatedPeriod,person).Where(s => inflatedPeriod.Contains(s.DateOnlyAsPeriod.DateOnly) );
			});

			var rule = new NewDayOffRule(new WorkTimeStartEndExtractor());

			var ruleResponse = rule.Validate(schedules, scheduleDays).Select(r => new BusinessRuleResponse(r.TypeOfRule, r.Message, r.Error, r.Mandatory,
				dateOnlyPeriod.ToDateTimePeriod(r.Person.PermissionInformation.DefaultTimeZone()), r.Person, r.DateOnlyPeriod));

			return ruleResponse;
		}
	}

	public interface IScheduleValidationProvider
	{
		IList<BusinessRuleValidationResult> GetBusinessRuleValidationResults(FetchRuleValidationResultFormData input, BusinessRuleFlags ruleFlags);
	}
}