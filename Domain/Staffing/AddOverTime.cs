using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class AddOverTime : IAddOverTime
	{
		private readonly ScheduledStaffingToDataSeries _scheduledStaffingToDataSeries;
		private readonly INow _now;
		private readonly IUserTimeZone _userTimeZone;
		private readonly CalculateOvertimeSuggestionProvider _calculateOvertimeSuggestionProvider;
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IPersonForOvertimeProvider _personForOvertimeProvider;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ScheduleOvertimeExecuteWrapper _scheduleOvertimeExecuteWrapper;


		public AddOverTime(ScheduledStaffingToDataSeries scheduledStaffingToDataSeries,
						   INow now, IUserTimeZone userTimeZone, CalculateOvertimeSuggestionProvider calculateOvertimeSuggestionProvider, 
						   IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository, ICommandDispatcher commandDispatcher, 
						   ISkillCombinationResourceRepository skillCombinationResourceRepository, 
						   IPersonForOvertimeProvider personForOvertimeProvider, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, 
						   IPersonRepository personRepository, ISkillRepository skillRepository, 
						   ScheduleOvertimeExecuteWrapper scheduleOvertimeExecuteWrapper)
		{
			_scheduledStaffingToDataSeries = scheduledStaffingToDataSeries;
			_now = now;
			_userTimeZone = userTimeZone;
			_calculateOvertimeSuggestionProvider = calculateOvertimeSuggestionProvider;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_commandDispatcher = commandDispatcher;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_personForOvertimeProvider = personForOvertimeProvider;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_personRepository = personRepository;
			_skillRepository = skillRepository;
			_scheduleOvertimeExecuteWrapper = scheduleOvertimeExecuteWrapper;
		}


		public OverTimeSuggestionResultModel GetSuggestion(OverTimeSuggestionModel overTimeSuggestionModel)
		{
			var result = new List<OverTimeModel>();
			//midnight shift ??
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(overTimeSuggestionModel.TimeSerie.Min(), _userTimeZone.TimeZone()), TimeZoneHelper.ConvertToUtc(overTimeSuggestionModel.TimeSerie.Max(), _userTimeZone.TimeZone()));
			var userDateOnly = new DateOnly(overTimeSuggestionModel.TimeSerie.Min());

			var personsModels = _personForOvertimeProvider.Persons(overTimeSuggestionModel.SkillIds, period.StartDateTime, period.EndDateTime);
			var persons = _personRepository.FindPeople(personsModels.Select(x => x.PersonId));
			
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(period), _currentScenario.Current(), new PersonProvider(persons), new ScheduleDictionaryLoadOptions(false, false), persons);

			var allSkills = _skillRepository.LoadAll().ToList();
			var multiplicationDefinition = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions().FirstOrDefault();

			var scheduleDays = persons.Select(person => scheduleDictionary[person].ScheduledDay(userDateOnly)).ToList();

			var skills = allSkills.Where(x => overTimeSuggestionModel.SkillIds.Contains(x.Id.GetValueOrDefault())).ToList();

			if (!skills.Any())
				return new OverTimeSuggestionResultModel
				{
					SuggestedStaffingWithOverTime = null,
					OverTimeModels = result
				};

			var activity = skills.FirstOrDefault().Activity;
			var overtimePreferences = new OvertimePreferences
			{
				SkillActivity = activity,
				ScheduleTag = new NullScheduleTag(),
				OvertimeType = multiplicationDefinition,
				SelectedTimePeriod = new TimePeriod(TimeSpan.FromMinutes(15), TimeSpan.FromHours(5))
			};
			
			var model = _scheduleOvertimeExecuteWrapper.Execute(overtimePreferences, new SchedulingProgress(), scheduleDays, period, allSkills);

			

			//var deltas = new List<SkillCombinationResource>();
			//foreach (var person in affectedPersons)
			//{
			//	var personDeltas = _scheduleDayDifferenceSaver.GetDifferences(scheduleDictionary[person]).ToList();
			//	result.Add(new OverTimeModel
			//	{
			//		PersonId = person.Id.GetValueOrDefault(),
			//		ActivityId = activity.Id.GetValueOrDefault(),
			//		Deltas = personDeltas,
			//		EndDateTime = personDeltas.Max(x => x.EndDateTime),
			//		StartDateTime = personDeltas.Min(x => x.StartDateTime)
			//	});
			//	deltas.AddRange(personDeltas);
			//}

			//foreach (var resource in combinationResources)
			//{
			//	var delta = deltas.Where(x => x.SkillCombination.SequenceEqual(resource.SkillCombination) && x.StartDateTime == resource.StartDateTime).Sum(x => x.Resource);
			//	resource.Resource += delta;
			//}

			//var staffingIntervals = _skillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(period, combinationResources, false).Where(x => x.SkillId == overTimeSuggestionModel.SkillIds.FirstOrDefault());
			var overTimescheduledStaffingPerSkill = model.ResourceCalculationPeriods.Where(s => s.SkillId == skills.FirstOrDefault().Id.GetValueOrDefault()).Select(x => new SkillStaffingIntervalLightModel
			{
				Id = x.SkillId,
				StartDateTime = TimeZoneHelper.ConvertFromUtc(x.StartDateTime, _userTimeZone.TimeZone()),
				EndDateTime = TimeZoneHelper.ConvertFromUtc(x.EndDateTime, _userTimeZone.TimeZone()),
				StaffingLevel = x.StaffingLevel
			}).ToList();

			return new OverTimeSuggestionResultModel
			{
				SuggestedStaffingWithOverTime = _scheduledStaffingToDataSeries.DataSeries(overTimescheduledStaffingPerSkill, overTimeSuggestionModel.TimeSerie),
				OverTimeModels = result
			};

			//return result;
		}

		public OverTimeSuggestionResultModel GetSuggestionOld(OverTimeSuggestionModel overTimeSuggestionModel)
		{
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _userTimeZone.TimeZone());
			var usersTomorrow = new DateOnly(usersNow.AddHours(24));
			var userstomorrowUtc = TimeZoneHelper.ConvertToUtc(usersTomorrow.Date, _userTimeZone.TimeZone());

			var overTimeStaffingSuggestion = _calculateOvertimeSuggestionProvider.GetOvertimeSuggestions(overTimeSuggestionModel.SkillIds, _now.UtcDateTime(), userstomorrowUtc);
			var overTimescheduledStaffingPerSkill = overTimeStaffingSuggestion.SkillStaffingIntervals.Select(x => new SkillStaffingIntervalLightModel
			{
				Id = x.SkillId,
				StartDateTime = TimeZoneHelper.ConvertFromUtc(x.StartDateTime, _userTimeZone.TimeZone()),
				EndDateTime = TimeZoneHelper.ConvertFromUtc(x.EndDateTime, _userTimeZone.TimeZone()),
				StaffingLevel = x.StaffingLevel
			}).ToList();
			return new OverTimeSuggestionResultModel
			{
				SuggestedStaffingWithOverTime = _scheduledStaffingToDataSeries.DataSeries(overTimescheduledStaffingPerSkill, overTimeSuggestionModel.TimeSerie),
				OverTimeModels = overTimeStaffingSuggestion.OverTimeModels
			};

		}


		public void Apply(IList<OverTimeModel> overTimeModels )
		{
			var multiplicationDefinition = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions().FirstOrDefault();
			if (multiplicationDefinition == null) return;

			foreach (var overTimeModel in overTimeModels)
			{
				_commandDispatcher.Execute(new AddOvertimeActivityCommand
										   {
											   ActivityId = overTimeModel.ActivityId,
											   Date = new DateOnly(overTimeModel.StartDateTime),
											   MultiplicatorDefinitionSetId = multiplicationDefinition.Id.GetValueOrDefault(),
											   Period = new DateTimePeriod(overTimeModel.StartDateTime.Utc(), overTimeModel.EndDateTime.Utc()),
											   PersonId = overTimeModel.PersonId
										   });

				//_skillCombinationResourceRepository.PersistChanges(overTimeModel.Deltas);
			}
		}
		
	}

	public interface IAddOverTime
	{
		OverTimeSuggestionResultModel GetSuggestionOld(OverTimeSuggestionModel overTimeSuggestionModel);
		OverTimeSuggestionResultModel GetSuggestion(OverTimeSuggestionModel overTimeSuggestionModel);
		void Apply(IList<OverTimeModel> overTimeModels);
	}

	public class OverTimeStaffingSuggestionModel
	{
		public IList<SkillStaffingInterval> SkillStaffingIntervals { get; set; }
		public IList<OverTimeModel> OverTimeModels { get; set; }
	}


	public class OverTimeSuggestionResultModel
	{
		public double?[] SuggestedStaffingWithOverTime { get; set; }
		public IList<OverTimeModel> OverTimeModels { get; set; }
	}

	public class OverTimeSuggestionModel
	{
		public IList<Guid> SkillIds { get; set; }
		public DateTime[] TimeSerie { get; set; }
	}

	public class OverTimeModel
	{
		public Guid ActivityId { get; set; }
		public Guid PersonId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public IList<SkillCombinationResource> Deltas { get; set; }
	}

}
