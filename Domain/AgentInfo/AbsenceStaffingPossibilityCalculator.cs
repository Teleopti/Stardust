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
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class AbsenceStaffingPossibilityCalculator : IAbsenceStaffingPossibilityCalculator
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISkillStaffingDataLoader _skillStaffingDataLoader;
		private readonly INow _now;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly ISkillStaffingIntervalUnderstaffing _skillStaffingIntervalUnderstaffing;

		public AbsenceStaffingPossibilityCalculator(ILoggedOnUser loggedOnUser,
			IScheduleStorage scheduleStorage,
			ICurrentScenario scenarioRepository,
			ISkillStaffingDataLoader skillStaffingDataLoader,
			INow now,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider,
			ISkillStaffingIntervalUnderstaffing skillStaffingIntervalUnderstaffing)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_skillStaffingDataLoader = skillStaffingDataLoader;
			_now = now;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_skillStaffingIntervalUnderstaffing = skillStaffingIntervalUnderstaffing;
		}

		public IList<CalculatedPossibilityModel> CalculateIntradayIntervalPossibilities(DateOnlyPeriod period)
		{
			var scheduleDictionary = loadScheduleDictionary(period);
			var skills = getSupportedPersonSkills(period).Select(s => s.Skill).ToArray();
			var useShrinkage = isShrinkageValidatorEnabled();
			var skillStaffingDatas = _skillStaffingDataLoader.Load(skills, period, useShrinkage, isCheckingIntradayStaffing);
			return calculatePossibilities(skillStaffingDatas, scheduleDictionary);
		}

		private bool isShrinkageValidatorEnabled()
		{
			var person = _loggedOnUser.CurrentUser();
			if (person.WorkflowControlSet?.AbsenceRequestOpenPeriods == null)
				return false;

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			return person.WorkflowControlSet.IsAbsenceRequestValidatorEnabled<StaffingThresholdWithShrinkageValidator>(timeZone);
		}

		private bool isCheckingIntradayStaffing(DateOnly date)
		{
			var workflowControlSet = _loggedOnUser.CurrentUser().WorkflowControlSet;
			if (workflowControlSet?.AbsenceRequestOpenPeriods == null || !workflowControlSet.AbsenceRequestOpenPeriods.Any())
			{
				return false;
			}

			return workflowControlSet.IsAbsenceRequestCheckStaffingByIntraday(_now.ServerDate_DontUse(), date);
		}

		private IScheduleDictionary loadScheduleDictionary(DateOnlyPeriod period)
		{
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] { _loggedOnUser.CurrentUser() },
				new ScheduleDictionaryLoadOptions(false, false) { LoadAgentDayScheduleTags = false },
				period,
				_scenarioRepository.Current());
			return scheduleDictionary;
		}

		private IList<CalculatedPossibilityModel> calculatePossibilities(
			IList<SkillStaffingData> skillStaffingDatas, IScheduleDictionary scheduleDictionary)
		{
			var resolution = skillStaffingDatas.FirstOrDefault()?.Resolution ?? 15;
			var calculatedPossibilityModels = new List<CalculatedPossibilityModel>();
			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Date);
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				calculatedPossibilityModels.Add(new CalculatedPossibilityModel
				{
					Date = skillStaffingDataGroup.Key,
					IntervalPossibilies = calculateIntervalPossibilities(skillStaffingDataGroup, scheduleDictionary),
					Resolution = resolution
				});
			}
			return calculatedPossibilityModels;
		}

		private Dictionary<DateTime, int> calculateIntervalPossibilities(IEnumerable<SkillStaffingData> skillStaffingDatas, IScheduleDictionary scheduleDictionary)
		{
			var intervalPossibilities = new Dictionary<DateTime, int>();
			var personAssignmentDictionary = skillStaffingDatas.Select(s => s.Date).Distinct()
				.ToDictionary(d => d, d => getPersonAssignment(scheduleDictionary, d));
			foreach (var skillStaffingData in skillStaffingDatas)
			{
				if (!staffingDataHasValue(skillStaffingData))
					continue;

				if (hasFairPossibilityInThisInterval(intervalPossibilities, skillStaffingData.Time))
					continue;
				
				substractUsersSchedule(skillStaffingData, personAssignmentDictionary[skillStaffingData.Date]);

				var possibility = calculatePossibility(skillStaffingData);
				var key = skillStaffingData.Time;
				intervalPossibilities[key] = possibility;
			}
			return intervalPossibilities;
		}

		private int calculatePossibility(SkillStaffingData skillStaffingData)
		{
			var isSatisfied = _skillStaffingIntervalUnderstaffing.IsSatisfiedBy(skillStaffingData.Skill, skillStaffingData.SkillStaffingInterval);
			return isSatisfied ? ScheduleStaffingPossibilityConsts.FairPossibility : ScheduleStaffingPossibilityConsts.GoodPossibility;
		}

		private static bool isSkillScheduled(IPersonAssignment personAssignment, DateTimePeriod period, ISkill skill)
		{
			if (isPersonAssignmentNullOrEmpty(personAssignment))
				return false;

			var mainActivities = personAssignment.MainActivities();
			var overtimeActivities = personAssignment.OvertimeActivities();

			var isSkillScheduled =
				mainActivities
					.Any(m => m.Payload.RequiresSkill && m.Payload == skill.Activity && m.Period.Intersect(period))
				|| overtimeActivities
					.Any(m => m.Payload.RequiresSkill && m.Payload == skill.Activity && m.Period.Intersect(period));

			return isSkillScheduled;
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

		private void substractUsersSchedule(SkillStaffingData skillStaffingData, IPersonAssignment personAssignment)
		{
			var skill = skillStaffingData.Skill;
			var timezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var startTime = TimeZoneHelper.ConvertToUtc(skillStaffingData.Time, timezone);
			var skillScheduled = isSkillScheduled(personAssignment,
				new DateTimePeriod(startTime, startTime.AddMinutes(skillStaffingData.Resolution)),
				skill);
			if (!skillScheduled) return;
			// we can't calculate current user's schedule for a skill in a specific period
			// so we just substract 1 which means user's schedule is removed(#44607)
			if (skillStaffingData.ScheduledStaffing <= 1 && !skill.StaffingThresholds.Understaffing.Value.Equals(-1))
			{
				skillStaffingData.ScheduledStaffing = 0;
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

		private IEnumerable<IPersonSkill> getSupportedPersonSkills(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (personPeriod.Length == 0)
				return new IPersonSkill[] { };

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Where(p => _supportedSkillsInIntradayProvider.CheckSupportedSkill(p.Skill)).ToArray();

			return personSkills.Distinct();
		}
	}
}