using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
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

		public ScheduleValidationProvider(IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
			IPersonRepository personRepository, IPersonWeekViolatingWeeklyRestSpecification personWeekViolating,
			IUserTimeZone timeZone, IPersonNameProvider personNameProvider)
		{
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_personRepository = personRepository;
			_personWeekViolating = personWeekViolating;
			_timeZone = timeZone;
			_personNameProvider = personNameProvider;
		}

		public IList<ActivityLayerOverlapCheckingResult> GetActivityLayerOverlapCheckingResult(
			CheckActivityLayerOverlapFormData input)
		{
			var results = new List<ActivityLayerOverlapCheckingResult>();

			var people = _personRepository.FindPeople(input.PersonIds);
			var scenario = _currentScenario.Current();
			var activityPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(input.StartTime, _timeZone.TimeZone()),
				TimeZoneHelper.ConvertToUtc(input.EndTime, _timeZone.TimeZone()));

			var schedulePeriod = (new DateOnlyPeriod(input.Date, input.Date)).Inflate(1);
			
			foreach (var person in people)
			{
				var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
					schedulePeriod,
					scenario);

				var scheduleDay = schedules[person].ScheduledDay(input.Date);
				var projection = scheduleDay.ProjectionService().CreateProjection();

				var overlapLayers = projection
					.Where(layer =>
					{
						var activityLayer = layer.Payload as IActivity;
						return activityLayer != null && layer.Period.Intersect(activityPeriod) && !activityLayer.AllowOverwrite;
					})
					.Select(layer => new OverlappedLayer
					{
						Name = ((IActivity) layer.Payload).Name,
						StartTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, _timeZone.TimeZone()),
						EndTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime,_timeZone.TimeZone())
					})
					.ToList();

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
			var scenario = _currentScenario.Current();		
			var schedulePeriod = (new DateOnlyPeriod(input.Date,input.Date)).Inflate(1);

			foreach(var personActivity in input.PersonActivities)
			{
				var person = _personRepository.Get(personActivity.PersonId);

				var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,new ScheduleDictionaryLoadOptions(false,false),
					schedulePeriod,
					scenario);

				var scheduleDay = schedules[person].ScheduledDay(input.Date);
				var personAssignment = scheduleDay.PersonAssignment().EntityClone();

				if (!personActivity.ShiftLayerIds.Any()) continue;

				var targetLayer =
					personAssignment.ShiftLayers.FirstOrDefault(layer => layer.Id == personActivity.ShiftLayerIds.First());

				if (targetLayer == null) continue;

				personAssignment.MoveActivityAndKeepOriginalPriority(targetLayer,TimeZoneHelper.ConvertToUtc(input.StartTime, _timeZone.TimeZone()), null);

				Func<IVisualLayer, bool> stickyLayerPredicate = layer =>
				{
					var activityLayer = layer.Payload as IActivity;
					return activityLayer != null && !activityLayer.AllowOverwrite;
				};

				var stickyLayersInNewProjection =
					personAssignment.ProjectionService().CreateProjection().Where(stickyLayerPredicate).ToList();

				var stickyLayersInOldProjection = scheduleDay.ProjectionService().CreateProjection().Where(stickyLayerPredicate).ToList();
				
				var overlapLayers = stickyLayersInOldProjection.Where(layer =>
				{
					return stickyLayersInNewProjection.All(l => l.Payload.Id != layer.Payload.Id || l.Period != layer.Period);
				})
				.Select(layer => new OverlappedLayer
				{
					Name = ((IActivity)layer.Payload).Name,
					StartTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime,_timeZone.TimeZone()),
					EndTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime,_timeZone.TimeZone())
				})
				.ToList();

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
							Warnings = x.Select(y => y.Message).ToList()
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
	}

	public class OverlappedLayer
	{
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

	public class ActivityLayerOverlapCheckingResult
	{
		public Guid PersonId { get; set; }
		public string Name { get; set; }
		public List<OverlappedLayer> OverlappedLayers { get; set; }
	}


}