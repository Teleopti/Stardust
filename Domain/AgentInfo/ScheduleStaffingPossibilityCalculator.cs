using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class ScheduleStaffingPossibilityCalculator : IScheduleStaffingPossibilityCalculator
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISkillStaffingDataLoader _skillStaffingDataLoader;
		private readonly INow _now;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly IPrimaryPersonSkillFilter _primaryPersonSkillFilter;


		public ScheduleStaffingPossibilityCalculator(ILoggedOnUser loggedOnUser, IScheduleStorage scheduleStorage,
			ICurrentScenario scenarioRepository, ISkillStaffingDataLoader skillStaffingDataLoader, INow now, ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider, IPrimaryPersonSkillFilter primaryPersonSkillFilter)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_skillStaffingDataLoader = skillStaffingDataLoader;
			_now = now;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_primaryPersonSkillFilter = primaryPersonSkillFilter;
		}

		public IList<CalculatedPossibilityModel> CalculateIntradayAbsenceIntervalPossibilities(DateOnlyPeriod period)
		{
			var scheduleDictionary = loadScheduleDictionary(period);
			var skills = getSupportedPersonSkills(period).Select(s => s.Skill).ToList();
			var skillStaffingDatas = _skillStaffingDataLoader.Load(skills, period, isCheckingIntradayStaffing);
			Func<ISkill, IValidatePeriod, bool> isSatisfied =
				(skill, validatePeriod) => new IntervalHasUnderstaffing(skill).IsSatisfiedBy(validatePeriod);
			return calculatePossibilities(skillStaffingDatas, isSatisfied, scheduleDictionary, true);
		}

		public IList<CalculatedPossibilityModel> CalculateIntradayOvertimeIntervalPossibilities(DateOnlyPeriod period)
		{
			var scheduleDictionary = loadScheduleDictionary(period);
			var skills = _primaryPersonSkillFilter.Filter(getSupportedPersonSkills(period)).Select(s => s.Skill).ToList();
			var skillStaffingDatas = _skillStaffingDataLoader.Load(skills, period);
			Func<ISkill, IValidatePeriod, bool> isSatisfied =
				(skill, validatePeriod) => !new IntervalHasSeriousUnderstaffing(skill).IsSatisfiedBy(validatePeriod);
			return calculatePossibilities(skillStaffingDatas, isSatisfied, scheduleDictionary);
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
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				_scenarioRepository.Current());
			return scheduleDictionary;
		}

		private IList<CalculatedPossibilityModel> calculatePossibilities(
			IList<SkillStaffingData> skillStaffingDatas,
			Func<ISkill, IValidatePeriod, bool> isSatisfied, IScheduleDictionary scheduleDictionary, bool ignoreCurrentUsersSchedule = false)
		{
			var resolution = skillStaffingDatas.FirstOrDefault()?.Resolution ?? 15;
			var calculatedPossibilityModels = new List<CalculatedPossibilityModel>();
			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Date);
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				calculatedPossibilityModels.Add(new CalculatedPossibilityModel
				{
					Date = skillStaffingDataGroup.Key,
					IntervalPossibilies = calculateIntervalPossibilities(skillStaffingDataGroup, isSatisfied, scheduleDictionary, ignoreCurrentUsersSchedule),
					Resolution = resolution
				});
			}
			return calculatedPossibilityModels;
		}

		private Dictionary<DateTime, int> calculateIntervalPossibilities(IEnumerable<SkillStaffingData> skillStaffingDatas,
			Func<ISkill, IValidatePeriod, bool> isSatisfied, IScheduleDictionary scheduleDictionary, bool ignoreCurrentUsersSchedule = false)
		{
			var intervalPossibilities = new Dictionary<DateTime, int>();
			foreach (var skillStaffingData in skillStaffingDatas)
			{
				var personAssignment = getPersonAssignment(scheduleDictionary, skillStaffingData.Date);

				if (!isPersonAssignmentNullOrEmpty(personAssignment) && !isSkillScheduled(personAssignment, skillStaffingData.Skill))
					continue;

				if (!staffingDataHasValue(skillStaffingData)) continue;

				if (hasFairPossibilityInThisInterval(intervalPossibilities, skillStaffingData.Time))
					continue;

				if (ignoreCurrentUsersSchedule)
					substractUsersSchedule(skillStaffingData, personAssignment);

				var possibility = calculatePossibility(skillStaffingData, isSatisfied);
				var key = skillStaffingData.Time;
				if (intervalPossibilities.ContainsKey(key))
				{
					intervalPossibilities[key] = possibility;
				}
				else
				{
					intervalPossibilities.Add(key, possibility);
				}
			}
			return intervalPossibilities;
		}

		private static int calculatePossibility(SkillStaffingData skillStaffingData, Func<ISkill, IValidatePeriod, bool> staffingSpecification)
		{
			var staffingInterval = new SkillStaffingInterval
			{
				CalculatedResource = skillStaffingData.ScheduledStaffing.Value,
				FStaff = skillStaffingData.ForecastedStaffing.Value
			};
			var isSatisfied = staffingSpecification(skillStaffingData.Skill, staffingInterval);
			return isSatisfied ? ScheduleStaffingPossibilityConsts.FairPossibility : ScheduleStaffingPossibilityConsts.GoodPossibility;
		}

		private static bool isSkillScheduled(IPersonAssignment personAssignment, ISkill skill)
		{
			var scheduledActivities = personAssignment.MainActivities().Select(m => m.Payload).Where(p => p.RequiresSkill);

			return scheduledActivities.Contains(skill.Activity);
		}

		private static bool isSkillScheduled(IPersonAssignment personAssignment, DateTimePeriod period, ISkill skill)
		{
			if (isPersonAssignmentNullOrEmpty(personAssignment))
				return false;

			var isSkillScheduled =
				personAssignment.MainActivities()
					.Any(m => m.Payload.RequiresSkill && m.Payload == skill.Activity && m.Period.Intersect(period));

			return isSkillScheduled;
		}

		private static IPersonAssignment getPersonAssignment(IScheduleDictionary scheduleDictionary, DateOnly date)
		{
			var scheduleDays = scheduleDictionary.SchedulesForDay(date).ToList();
			var scheduleDay = scheduleDays.Any() ? scheduleDays.First() : null;
			var personAssignment = scheduleDay?.PersonAssignment();
			return personAssignment;
		}

		private static bool isPersonAssignmentNullOrEmpty(IPersonAssignment personAssignment)
		{
			return personAssignment == null || personAssignment.ShiftLayers.IsEmpty();
		}

		private void substractUsersSchedule(SkillStaffingData skillStaffingData, IPersonAssignment personAssignment)
		{
			var timezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var startTime = TimeZoneHelper.ConvertToUtc(skillStaffingData.Time, timezone);
			var skillScheduled = isSkillScheduled(personAssignment,
				new DateTimePeriod(startTime, startTime.AddMinutes(skillStaffingData.Resolution)),
				skillStaffingData.Skill);
			if (!skillScheduled) return;
			// we can't calculate current user's schedule for a skill in a specific period
			// so we just substract 1 which means user's schedule is removed(#44607)
			skillStaffingData.ScheduledStaffing -= 1;
		}

		private static bool hasFairPossibilityInThisInterval(Dictionary<DateTime, int> intervalPossibilities, DateTime time)
		{
			int possibility;
			return intervalPossibilities.TryGetValue(time, out possibility) && possibility == ScheduleStaffingPossibilityConsts.FairPossibility;
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
			if (!personPeriod.Any())
				return new IPersonSkill[] { };

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Where(p => _supportedSkillsInIntradayProvider.CheckSupportedSkill(p.Skill)).ToArray();

			return !personSkills.Any() ? new IPersonSkill[] { } : personSkills.Distinct();
		}

	}
}
