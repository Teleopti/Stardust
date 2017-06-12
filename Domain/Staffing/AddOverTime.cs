using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
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
		private readonly IUserTimeZone _timeZone;
		private readonly CalculateOvertimeSuggestionProvider _calculateOvertimeSuggestionProvider;
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ScheduleOvertime _scheduleOvertime;
		private readonly IPersonForOvertimeProvider _personForOvertimeProvider;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolderFunc;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IScheduleDayDifferenceSaver _scheduleDayDifferenceSaver;
		private readonly ISkillStaffingIntervalProvider _skillStaffingIntervalProvider;


		public AddOverTime(ScheduledStaffingToDataSeries scheduledStaffingToDataSeries,
						   INow now, IUserTimeZone timeZone, CalculateOvertimeSuggestionProvider calculateOvertimeSuggestionProvider, 
						   IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository, ICommandDispatcher commandDispatcher, 
						   ISkillCombinationResourceRepository skillCombinationResourceRepository, ScheduleOvertime scheduleOvertime, 
						   IPersonForOvertimeProvider personForOvertimeProvider, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, 
						   IPersonRepository personRepository, ISkillRepository skillRepository, Func<ISchedulerStateHolder> schedulerStateHolderFunc, 
						   ISkillDayLoadHelper skillDayLoadHelper, IScheduleDayDifferenceSaver scheduleDayDifferenceSaver, ISkillStaffingIntervalProvider skillStaffingIntervalProvider)
		{
			_scheduledStaffingToDataSeries = scheduledStaffingToDataSeries;
			_now = now;
			_timeZone = timeZone;
			_calculateOvertimeSuggestionProvider = calculateOvertimeSuggestionProvider;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_commandDispatcher = commandDispatcher;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_scheduleOvertime = scheduleOvertime;
			_personForOvertimeProvider = personForOvertimeProvider;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_personRepository = personRepository;
			_skillRepository = skillRepository;
			_schedulerStateHolderFunc = schedulerStateHolderFunc;
			_skillDayLoadHelper = skillDayLoadHelper;
			_scheduleDayDifferenceSaver = scheduleDayDifferenceSaver;
			_skillStaffingIntervalProvider = skillStaffingIntervalProvider;
		}


		public OverTimeSuggestionResultModel GetSuggestion(OverTimeSuggestionModel overTimeSuggestionModel)
		{
			//midnight shift ??
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(overTimeSuggestionModel.TimeSerie.Min(), _timeZone.TimeZone()), TimeZoneHelper.ConvertToUtc(overTimeSuggestionModel.TimeSerie.Max(), _timeZone.TimeZone()));
			var userDateOnly = new DateOnly(overTimeSuggestionModel.TimeSerie.Min());

			var personsModels = _personForOvertimeProvider.Persons(overTimeSuggestionModel.SkillIds, period.StartDateTime, period.EndDateTime);
			var persons = _personRepository.FindPeople(personsModels.Select(x => x.PersonId));
			
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(period), _currentScenario.Current(), new PersonProvider(persons), new ScheduleDictionaryLoadOptions(false, false), persons);
			var scheduleDays = new List<IScheduleDay>();
	 
			var skills = _skillRepository.LoadAll().ToList();
			var multiplicationDefinition = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions().FirstOrDefault();

			foreach (var person in persons)
			{
				scheduleDays.Add(scheduleDictionary[person].ScheduledDay(userDateOnly));
			}
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			var stateHolder = _schedulerStateHolderFunc();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(_timeZone.TimeZone());
			stateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(dateOnlyPeriod, _timeZone.TimeZone());
			stateHolder.SchedulingResultState.Schedules = scheduleDictionary;
			stateHolder.SchedulingResultState.SkillDays =
				_skillDayLoadHelper.LoadSchedulerSkillDays(dateOnlyPeriod, skills, _currentScenario.Current());
			stateHolder.SchedulingResultState.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
			stateHolder.SchedulingResultState.AddSkills(skills.ToArray());
			var activity = skills.FirstOrDefault(x => x.Id.GetValueOrDefault() == overTimeSuggestionModel.SkillIds.FirstOrDefault()).Activity;

			 _scheduleOvertime.Execute(new OvertimePreferences { SkillActivity = activity, ScheduleTag = new NullScheduleTag(), OvertimeType = multiplicationDefinition, SelectedTimePeriod = new TimePeriod(TimeSpan.FromMinutes(15), TimeSpan.FromHours(5)), SelectedSpecificTimePeriod = new TimePeriod(TimeSpan.Zero, new TimeSpan(1, 0, 0, 0)) }, new SchedulingProgress(), scheduleDays);
			
			var result = new List<OverTimeModel>();
			var deltas = new List<SkillCombinationResource>();
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

			foreach (var resource in combinationResources)
			{
				var delta = deltas.Where(x => x.SkillCombination.SequenceEqual(resource.SkillCombination) && x.StartDateTime == resource.StartDateTime).Sum(x => x.Resource);
				resource.Resource += delta;
			}

			var staffingIntervals = _skillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(period, combinationResources, false).Where(x => x.SkillId == overTimeSuggestionModel.SkillIds.FirstOrDefault() );
			var overTimescheduledStaffingPerSkill = staffingIntervals.Select(x => new SkillStaffingIntervalLightModel
			{
				Id = x.SkillId,
				StartDateTime = TimeZoneHelper.ConvertFromUtc(x.StartDateTime, _timeZone.TimeZone()),
				EndDateTime = TimeZoneHelper.ConvertFromUtc(x.EndDateTime, _timeZone.TimeZone()),
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
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var usersTomorrow = new DateOnly(usersNow.AddHours(24));
			var userstomorrowUtc = TimeZoneHelper.ConvertToUtc(usersTomorrow.Date, _timeZone.TimeZone());

			var overTimeStaffingSuggestion = _calculateOvertimeSuggestionProvider.GetOvertimeSuggestions(overTimeSuggestionModel.SkillIds, _now.UtcDateTime(), userstomorrowUtc);
			var overTimescheduledStaffingPerSkill = overTimeStaffingSuggestion.SkillStaffingIntervals.Select(x => new SkillStaffingIntervalLightModel
			{
				Id = x.SkillId,
				StartDateTime = TimeZoneHelper.ConvertFromUtc(x.StartDateTime, _timeZone.TimeZone()),
				EndDateTime = TimeZoneHelper.ConvertFromUtc(x.EndDateTime, _timeZone.TimeZone()),
				StaffingLevel = x.StaffingLevel
			}).ToList();
			return new OverTimeSuggestionResultModel
			{
				SuggestedStaffingWithOverTime = _scheduledStaffingToDataSeries.DataSeries(overTimescheduledStaffingPerSkill, overTimeSuggestionModel.TimeSerie),
				OverTimeModels = overTimeStaffingSuggestion.OverTimeModels
			};

		}

		private static IDisposable getContext(IEnumerable<SkillCombinationResource> combinationResources, List<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataConatainerFromSkillCombinations(combinationResources.ToList(), skills, useAllSkills)));
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
