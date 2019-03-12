using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class AbsenceStaffingPossibilityCalculator : IAbsenceStaffingPossibilityCalculator
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISkillStaffingDataLoader _skillStaffingDataLoader;
		private readonly INow _now;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly ISkillStaffingIntervalUnderstaffing _skillStaffingIntervalUnderStaffing;

		public AbsenceStaffingPossibilityCalculator(IScheduleStorage scheduleStorage,
			ICurrentScenario scenarioRepository,
			ISkillStaffingDataLoader skillStaffingDataLoader,
			INow now,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			ISkillStaffingIntervalUnderstaffing skillStaffingIntervalUnderStaffing)
		{
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_skillStaffingDataLoader = skillStaffingDataLoader;
			_now = now;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_skillStaffingIntervalUnderStaffing = skillStaffingIntervalUnderStaffing;
		}

		public CalculatedPossibilityModelResult CalculateIntradayIntervalPossibilities(IPerson person,
			DateOnlyPeriod period)
		{
			var dayWithYesterday = period.StartDate.AddDays(-1);
			var enlargedPeriod = new DateOnlyPeriod(dayWithYesterday, period.EndDate);
			var scheduleDictionary = loadScheduleDictionary(person, enlargedPeriod);
			var filteredVisualLayersDictionary =
				loadMergedVisualLayers(person, scheduleDictionary, enlargedPeriod.DayCollection());
			var skills = getSupportedPersonSkills(person, period).Select(s => s.Skill).ToArray();
			var useShrinkageDic = getShrinkageStatusAccordingToPeriods(person, period);
			var workflowControlSet = person.WorkflowControlSet;
			var currentLocalDate = _now.CurrentLocalDate(person.PermissionInformation.DefaultTimeZone());
			var workflowControlSetHasOpenAbsenceRequestPeriods =
				workflowControlSet?.AbsenceRequestOpenPeriods != null &&
				workflowControlSet.AbsenceRequestOpenPeriods.Any();

			var skillStaffingData = _skillStaffingDataLoader.Load(skills, period, useShrinkageDic, date =>
				workflowControlSetHasOpenAbsenceRequestPeriods &&
				workflowControlSet.IsAbsenceRequestCheckStaffingByIntraday(currentLocalDate, date));

			return new CalculatedPossibilityModelResult(scheduleDictionary,
				calculatePossibilities(person, skillStaffingData, filteredVisualLayersDictionary));
		}

		private Dictionary<DateOnly, bool> getShrinkageStatusAccordingToPeriods(IPerson person, DateOnlyPeriod period)
		{
			var today = _now.CurrentLocalDate(person.PermissionInformation.DefaultTimeZone());
			var useShrinkageDic = period.DayCollection().ToDictionary(d => d, dateOnly =>
				person.WorkflowControlSet?.AbsenceRequestOpenPeriods != null && person.WorkflowControlSet
					.IsAbsenceRequestCheckStaffingByIntradayWithShrinkage(
						today, dateOnly));

			return useShrinkageDic;
		}

		private IScheduleDictionary loadScheduleDictionary(IPerson person, DateOnlyPeriod period)
		{
			var loadOption = new ScheduleDictionaryLoadOptions(false, false) { LoadAgentDayScheduleTags = false };
			var scenario = _scenarioRepository.Current();

			var scheduleDictionary =
				_scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { person }, loadOption, period, scenario);
			return scheduleDictionary;
		}

		private IDictionary<DateOnly, IFilteredVisualLayerCollection> loadMergedVisualLayers(IPerson person,IScheduleDictionary scheduleDictionary, IList<DateOnly> dayCollection)
		{
			var filteredVisualLayersDictionary = new Dictionary<DateOnly, IFilteredVisualLayerCollection>();

			foreach (var day in dayCollection)
			{
				var personAssignment = getPersonAssignment(scheduleDictionary, day);
				if (!isPersonAssignmentNullOrEmpty(personAssignment))
				{
					var timezone = person.PermissionInformation.DefaultTimeZone();
					var filteredVisualLayers = personAssignment.ProjectionService().CreateProjection()
						.FilterLayers(day.ToDateTimePeriod(timezone));
					filteredVisualLayersDictionary.Add(day, filteredVisualLayers);
				}
				else
				{
					filteredVisualLayersDictionary.Add(day, null);
				}
			}

			return filteredVisualLayersDictionary;
		}

		private IList<CalculatedPossibilityModel> calculatePossibilities(IPerson person,
			IList<SkillStaffingData> skillStaffingData, IDictionary<DateOnly, IFilteredVisualLayerCollection> filteredVisualLayersDictionary)
		{
			var resolution = skillStaffingData.FirstOrDefault()?.Resolution ?? 15;
			var skillStaffingDataGroups = skillStaffingData.GroupBy(s => s.Date);
			return skillStaffingDataGroups.Select(skillStaffingDataGroup => new CalculatedPossibilityModel
			{
				Date = skillStaffingDataGroup.Key,
				IntervalPossibilies =
					calculateIntervalPossibilities(person, skillStaffingDataGroup.ToList(), filteredVisualLayersDictionary),
				Resolution = resolution
			}).ToList();
		}

		private Dictionary<DateTime, int> calculateIntervalPossibilities(IPerson person, IList<SkillStaffingData> skillStaffingData, IDictionary<DateOnly, IFilteredVisualLayerCollection> filteredVisualLayersDictionary)
		{
			var intervalPossibilities = new Dictionary<DateTime, int>();

			foreach (var skillStaffing in skillStaffingData)
			{
				if (!staffingDataHasValue(skillStaffing))
					continue;

				if (hasFairPossibilityInThisInterval(intervalPossibilities, skillStaffing.Time))
					continue;

				if (isSkillScheduledInThisInterval(person, skillStaffing, filteredVisualLayersDictionary))
				{
					subtractUsersSchedule(skillStaffing);
				}

				var possibility = calculatePossibility(skillStaffing);
				var key = skillStaffing.Time;
				intervalPossibilities[key] = possibility;
			}
			return intervalPossibilities;
		}

		private int calculatePossibility(SkillStaffingData skillStaffingData)
		{
			var isSatisfied = _skillStaffingIntervalUnderStaffing.IsSatisfiedBy(skillStaffingData.Skill, skillStaffingData.SkillStaffingInterval);
			return isSatisfied ? ScheduleStaffingPossibilityConsts.FairPossibility : ScheduleStaffingPossibilityConsts.GoodPossibility;
		}

		private static bool isSkillScheduled(IFilteredVisualLayerCollection filteredVisualLayers, DateTimePeriod period, ISkill skill)
		{
			if (filteredVisualLayers == null)
				return false;

			var visualLayers = filteredVisualLayers.Where(x => x.Period.Contains(period)).ToList();
			if (visualLayers.Any() && visualLayers[0].Payload.Id == skill.Activity.Id)
				return true;

			return false;
		}

		private static IPersonAssignment getPersonAssignment(IScheduleDictionary scheduleDictionary, DateOnly date)
		{
			var scheduleDays = scheduleDictionary.SchedulesForDay(date);
			var scheduleDay = scheduleDays.FirstOrDefault();
			var personAssignment = scheduleDay?.PersonAssignment();
			return personAssignment;
		}

		private static bool isPersonAssignmentNullOrEmpty(IPersonAssignment personAssignment)
		{
			return personAssignment == null || personAssignment.ShiftLayers.IsEmpty();
		}

		private bool isSkillScheduledInThisInterval(IPerson person, SkillStaffingData skillStaffingData, IDictionary<DateOnly, IFilteredVisualLayerCollection> filteredVisualLayersDictionary)
		{
			var scheduleDate = skillStaffingData.Date;
			
			var skill = skillStaffingData.Skill;
			var timezone = person.PermissionInformation.DefaultTimeZone();
			var startTime = TimeZoneHelper.ConvertToUtc(skillStaffingData.Time, timezone);
			var period = new DateTimePeriod(startTime, startTime.AddMinutes(skillStaffingData.Resolution));

			var filteredVisualLayers = filteredVisualLayersDictionary[scheduleDate];

			return isSkillScheduled(filteredVisualLayers, period, skill);
		}

		private void subtractUsersSchedule(SkillStaffingData skillStaffingData)
		{
			if (skillStaffingData.ScheduledStaffing <= 1)
			{
				skillStaffingData.ScheduledStaffing = 0;
			}
			else
			{
				skillStaffingData.ScheduledStaffing -= 1;
			}
		}

		private static bool hasFairPossibilityInThisInterval(Dictionary<DateTime, int> intervalPossibilities, DateTime time)
		{
			return intervalPossibilities.TryGetValue(time, out var possibility) && possibility == ScheduleStaffingPossibilityConsts.FairPossibility;
		}

		private static bool staffingDataHasValue(SkillStaffingData skillStaffingData)
		{
			var isScheduledStaffingDataAvailable = skillStaffingData.ScheduledStaffing.HasValue;
			var isForecastedStaffingDataAvailable = skillStaffingData.ForecastedStaffing.HasValue;
			return isScheduledStaffingDataAvailable && isForecastedStaffingDataAvailable;
		}

		private IEnumerable<IPersonSkill> getSupportedPersonSkills(IPerson person, DateOnlyPeriod period)
		{
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (personPeriod.Length == 0)
				return new IPersonSkill[] { };

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Where(p => _supportedSkillsInIntradayProvider.CheckSupportedSkill(p.Skill)).ToArray();

			return personSkills.Distinct();
		}
	}
}