using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class ScheduleStaffingPossibilityCalculator : IScheduleStaffingPossibilityCalculator
	{
		private const int goodPossibility = 1;
		private const int fairPossibility = 0;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISkillStaffingDataLoader _skillStaffingDataLoader;

		public ScheduleStaffingPossibilityCalculator(ILoggedOnUser loggedOnUser, IScheduleStorage scheduleStorage,
			ICurrentScenario scenarioRepository, ISkillStaffingDataLoader skillStaffingDataLoader)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_skillStaffingDataLoader = skillStaffingDataLoader;
		}

		public IList<CalculatedPossibilityModel> CalculateIntradayAbsenceIntervalPossibilities(DateOnlyPeriod period)
		{
			var scheduleDictionary = loadScheduleDictionary(period);
			var skillStaffingDatas = _skillStaffingDataLoader.Load(period);
			Func<ISkill, IValidatePeriod, bool> isSatisfied =
				(skill, validatePeriod) => new IntervalHasUnderstaffing(skill).IsSatisfiedBy(validatePeriod);
			return calculatePossibilities(skillStaffingDatas, isSatisfied, scheduleDictionary);
		}

		public IList<CalculatedPossibilityModel> CalculateIntradayOvertimeIntervalPossibilities(DateOnlyPeriod period)
		{
			var scheduleDictionary = loadScheduleDictionary(period);
			var skillStaffingDatas = _skillStaffingDataLoader.Load(period);
			Func<ISkill, IValidatePeriod, bool> isSatisfied =
				(skill, validatePeriod) => !new IntervalHasSeriousUnderstaffing(skill).IsSatisfiedBy(validatePeriod);
			return calculatePossibilities(skillStaffingDatas, isSatisfied, scheduleDictionary);
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

		private static IList<CalculatedPossibilityModel> calculatePossibilities(
			IList<SkillStaffingData> skillStaffingDatas,
			Func<ISkill, IValidatePeriod, bool> isSatisfied, IScheduleDictionary scheduleDictionary)
		{
			var resolution = skillStaffingDatas.FirstOrDefault()?.Resolution ?? 15;
			var calculatedPossibilityModels = new List<CalculatedPossibilityModel>();
			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Date);
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				calculatedPossibilityModels.Add(new CalculatedPossibilityModel
				{
					Date = skillStaffingDataGroup.Key,
					IntervalPossibilies = calculateIntervalPossibilities(skillStaffingDataGroup, isSatisfied, scheduleDictionary),
					Resolution = resolution
				});
			}
			return calculatedPossibilityModels;
		}

		private static Dictionary<DateTime, int> calculateIntervalPossibilities(IEnumerable<SkillStaffingData> skillStaffingDatas,
			Func<ISkill, IValidatePeriod, bool> isSatisfied, IScheduleDictionary scheduleDictionary)
		{
			var intervalPossibilities = new Dictionary<DateTime, int>();
			foreach (var skillStaffingData in skillStaffingDatas)
			{
				if (!isSkillScheduled(scheduleDictionary, skillStaffingData.Date, skillStaffingData.Skill))
					continue;

				if (!staffingDataHasValue(skillStaffingData)) continue;

				if (hasFairPossibilityInThisInterval(intervalPossibilities, skillStaffingData.Time))
					continue;

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
			return isSatisfied ? fairPossibility : goodPossibility;
		}

		private static bool isSkillScheduled(IScheduleDictionary scheduleDictionary, DateOnly date, ISkill skill)
		{
			var scheduleDays = scheduleDictionary.SchedulesForDay(date).ToList();
			var scheduleDay = scheduleDays.Any() ? scheduleDays.First() : null;
			var personAssignment = scheduleDay?.PersonAssignment();
			if (personAssignment == null || personAssignment.ShiftLayers.IsEmpty())
				return true;

			var scheduledActivities = personAssignment.MainActivities().Select(m => m.Payload).Where(p => p.RequiresSkill);
			return scheduledActivities.Contains(skill.Activity);
		}

		private static bool hasFairPossibilityInThisInterval(Dictionary<DateTime, int> intervalPossibilities, DateTime time)
		{
			int possibility;
			return intervalPossibilities.TryGetValue(time, out possibility) && possibility == fairPossibility;
		}

		private static bool staffingDataHasValue(SkillStaffingData skillStaffingData)
		{
			var isScheduledStaffingDataAvailable = skillStaffingData.ScheduledStaffing.HasValue;
			var isForecastedStaffingDataAvailable = skillStaffingData.ForecastedStaffing.HasValue;
			return isScheduledStaffingDataAvailable && isForecastedStaffingDataAvailable;
		}

	}
}
