using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;
using static Teleopti.Interfaces.Domain.DateHelper;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class ScheduleValidationProvider : IScheduleValidationProvider
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonWeekViolatingWeeklyRestSpecification _personWeekViolating;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IUserTimeZone _timeZone;
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly INonoverwritableLayerChecker _nonoverwritableLayerChecker;

		public ScheduleValidationProvider(IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
			IPersonRepository personRepository, IPersonWeekViolatingWeeklyRestSpecification personWeekViolating,
			IUserTimeZone timeZone, IPersonNameProvider personNameProvider, IProxyForId<IActivity> activityForId, INonoverwritableLayerChecker nonoverwritableLayerChecker)
		{
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_personRepository = personRepository;
			_personWeekViolating = personWeekViolating;
			_timeZone = timeZone;
			_personNameProvider = personNameProvider;
			_activityForId = activityForId;
			_nonoverwritableLayerChecker = nonoverwritableLayerChecker;
		}

		public IList<ActivityLayerOverlapCheckingResult> GetActivityLayerOverlapCheckingResult(
			CheckActivityLayerOverlapFormData input)
		{
			var results = new List<ActivityLayerOverlapCheckingResult>();
			var people = _personRepository.FindPeople(input.PersonIds);
			var timezone = _timeZone.TimeZone();

			foreach (var person in people)
			{
				var activity = _activityForId.Load(input.ActivityId);
				var periodInUtc = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(input.StartTime, timezone), TimeZoneHelper.ConvertToUtc(input.EndTime, timezone));
				var overlapLayers = _nonoverwritableLayerChecker.GetOverlappedLayersWhenAddingActivity(person, input.Date, activity, periodInUtc);

				if(overlapLayers.IsEmpty()) continue;

				results.Add(new ActivityLayerOverlapCheckingResult
				{
					PersonId = person.Id.GetValueOrDefault(),
					Name = _personNameProvider.BuildNameFromSetting(person.Name),
					OverlappedLayers = overlapLayers
				});

			}

			return results;
		}

		public IList<ActivityLayerOverlapCheckingResult> GetMoveActivityLayerOverlapCheckingResult(
			CheckMoveActivityLayerOverlapFormData input)
		{
			var results = new List<ActivityLayerOverlapCheckingResult>();

			foreach (var personActivity in input.PersonActivities)
			{
				var person = _personRepository.Get(personActivity.PersonId);

				var newStartTimeInUtc = TimeZoneHelper.ConvertToUtc(input.StartTime, _timeZone.TimeZone());
				var overlapLayers = _nonoverwritableLayerChecker.GetOverlappedLayersWhenMovingActivity(person, input.Date,
					personActivity.ShiftLayerIds.ToArray(), newStartTimeInUtc);

				if (overlapLayers.IsEmpty()) continue;

				results.Add(new ActivityLayerOverlapCheckingResult
				{
					PersonId = person.Id.GetValueOrDefault(),
					Name = _personNameProvider.BuildNameFromSetting(person.Name),
					OverlappedLayers = overlapLayers
				});

			}

			return results;
		}

		public IList<string> GetAllValidationRuleTypes(BusinessRuleFlags ruleFlags)
		{
			var rules = new List<Type>();

			if(ruleFlags.HasFlag(BusinessRuleFlags.NewNightlyRestRule))
			{
				rules.Add(typeof(NewNightlyRestRule));
			}
			if(ruleFlags.HasFlag(BusinessRuleFlags.NewDayOffRule))
			{
				rules.Add(typeof(NewDayOffRule));
			}
			if(ruleFlags.HasFlag(BusinessRuleFlags.NotOverwriteLayerRule))
			{
				rules.Add(typeof(NotOverwriteLayerRule));
			}
			if(ruleFlags.HasFlag(BusinessRuleFlags.MinWeekWorkTimeRule))
			{
				rules.Add(typeof(MinWeekWorkTimeRule));
			}
			if(ruleFlags.HasFlag(BusinessRuleFlags.NewMaxWeekWorkTimeRule))
			{
				rules.Add(typeof(NewMaxWeekWorkTimeRule));
			}
			if(ruleFlags.HasFlag(BusinessRuleFlags.MinWeeklyRestRule))
			{
				rules.Add(typeof(MinWeeklyRestRule));				
			}
			
			return rules.Select(getValidationRuleName).ToList();
		}

		private string getValidationRuleName(Type rule)
		{
			return rule.Name + "Name";
		}

		public IList<BusinessRuleValidationResult> GetBusinessRuleValidationResults(FetchRuleValidationResultFormData input,
			BusinessRuleFlags ruleFlags)
		{
			var personIds = input.PersonIds;
			var people = _personRepository.FindPeople(personIds);
			var date = new DateOnly(input.Date);
			var dateOnlyPeriod = new DateOnlyPeriod(date, date);
			var scenario = _currentScenario.Current();
			var inflatedPeriod = dateOnlyPeriod.Inflate(1);
			var personPeriods = people.Select(person => GetWeekPeriod(date, person.FirstDayOfWeek)).ToList();
			personPeriods.Add(inflatedPeriod);
			var extendedPeriod = new DateOnlyPeriod(personPeriods.Min(p => p.StartDate), personPeriods.Max(p => p.EndDate));

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
			if (ruleFlags.HasFlag(BusinessRuleFlags.MinWeeklyRestRule))
			{
				rules.Add(new MinWeeklyRestRule(new WeeksFromScheduleDaysExtractor(), _personWeekViolating));
				extendedPeriod = extendedPeriod.Inflate(1);
			}
			if (ruleFlags.HasFlag(BusinessRuleFlags.NotOverwriteLayerRule))
			{
				rules.Add(new NotOverwriteLayerRule());
			}

			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people,
				new ScheduleDictionaryLoadOptions(false, false),
				extendedPeriod,
				scenario);

			var scheduleDays = people.SelectMany(person =>
			{
				return schedules.SchedulesForPeriod(dateOnlyPeriod, person).Where(s => s.DateOnlyAsPeriod.DateOnly == date);
			});

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
							Warnings = x.Select(y => new WarningInfo
							{
								Content = y.Message,
								RuleType = getValidationRuleName(y.TypeOfRule)
							}).ToList()
						};
					})
					.ToList();
			return businessRuleValidationResults;
		}

		private IEnumerable<IBusinessRuleResponse> checkDayOff(DateOnly date, IEnumerable<IPerson> people)
		{
			var dateOnlyPeriod = new DateOnlyPeriod(date, date);
			var scenario = _currentScenario.Current();
			var inflatedPeriod = dateOnlyPeriod.Inflate(1);

			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(people,
				new ScheduleDictionaryLoadOptions(false, false),
				inflatedPeriod,
				scenario);

			var scheduleDays = people.SelectMany(person =>
			{
				return
					schedules.SchedulesForPeriod(inflatedPeriod, person)
						.Where(s => inflatedPeriod.Contains(s.DateOnlyAsPeriod.DateOnly));
			});

			var rule = new NewDayOffRule(new WorkTimeStartEndExtractor());

			var ruleResponse =
				rule.Validate(schedules, scheduleDays)
					.Select(r => new BusinessRuleResponse(r.TypeOfRule, r.Message, r.Error, r.Mandatory,
						dateOnlyPeriod.ToDateTimePeriod(r.Person.PermissionInformation.DefaultTimeZone()), r.Person, r.DateOnlyPeriod));

			return ruleResponse;
		}
	}

	public interface IScheduleValidationProvider
	{
		IList<BusinessRuleValidationResult> GetBusinessRuleValidationResults(FetchRuleValidationResultFormData input,
			BusinessRuleFlags ruleFlags);

		IList<ActivityLayerOverlapCheckingResult> GetActivityLayerOverlapCheckingResult(
			CheckActivityLayerOverlapFormData input);

		IList<ActivityLayerOverlapCheckingResult> GetMoveActivityLayerOverlapCheckingResult(
			CheckMoveActivityLayerOverlapFormData input);

		IList<string> GetAllValidationRuleTypes(BusinessRuleFlags ruleFlags);
	}

	public class ActivityLayerOverlapCheckingResult
	{
		public Guid PersonId { get; set; }
		public string Name { get; set; }
		public IList<OverlappedLayer> OverlappedLayers { get; set; }
	}
}