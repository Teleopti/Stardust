using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class OvertimeStaffingPossibilityCalculator : IOvertimeStaffingPossibilityCalculator
	{
		private readonly ISkillStaffingDataLoader _skillStaffingDataLoader;
		private readonly IOvertimeRequestSkillProvider _overtimeRequestSkillProvider;
		private readonly ISkillStaffingDataSkillTypeFilter _skillStaffingDataSkillTypeFilter;

		public OvertimeStaffingPossibilityCalculator(ISkillStaffingDataLoader skillStaffingDataLoader,
			IOvertimeRequestSkillProvider overtimeRequestSkillProvider,
			ISkillStaffingDataSkillTypeFilter skillStaffingDataSkillTypeFilter)
		{
			_skillStaffingDataLoader = skillStaffingDataLoader;
			_overtimeRequestSkillProvider = overtimeRequestSkillProvider;
			_skillStaffingDataSkillTypeFilter = skillStaffingDataSkillTypeFilter;
		}

		public IList<CalculatedPossibilityModel> CalculateIntradayIntervalPossibilities(IPerson person, DateOnlyPeriod period, bool satisfyAllSkills)
		{
			var defaultTimeZone = person.PermissionInformation.DefaultTimeZone();

			var skills = period.DayCollection().Select(date =>
					_overtimeRequestSkillProvider
						.GetAvailableSkillsBySkillType(person, date.ToDateTimePeriod(defaultTimeZone)).ToList())
				.SelectMany(s => s);

			var useShrinkage = person.WorkflowControlSet.OvertimeRequestStaffingCheckMethod ==
							OvertimeRequestStaffingCheckMethod.IntradayWithShrinkage;

			var skillStaffingDatas = _skillStaffingDataLoader.Load(skills.Distinct().ToList(), period, useShrinkage,
				date =>
				{
					var siteOpenHour = person.SiteOpenHour(date);
					return siteOpenHour == null || !siteOpenHour.IsClosed;
				});

			var filteredSkillStaffingDatas = _skillStaffingDataSkillTypeFilter.Filter(skillStaffingDatas);

			return calculatePossibilities(filteredSkillStaffingDatas, satisfyAllSkills);
		}

		private IList<CalculatedPossibilityModel> calculatePossibilities(
			IList<SkillStaffingData> skillStaffingDatas, bool satisfyAllSkills)
		{
			var resolution = skillStaffingDatas.FirstOrDefault()?.Resolution ?? 15;
			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Date);

			return skillStaffingDataGroups.Select(skillStaffingDataGroup => new CalculatedPossibilityModel
			{
				Date = skillStaffingDataGroup.Key,
				IntervalPossibilies = calculateIntervalPossibilities(skillStaffingDataGroup, satisfyAllSkills),
				Resolution = resolution
			}).ToList();
		}

		private Dictionary<DateTime, int> calculateIntervalPossibilities(IEnumerable<SkillStaffingData> skillStaffingDatas, bool satisfyAllSkills)
		{
			var intervalPossibilities = new Dictionary<DateTime, int>();
			foreach (var skillStaffingData in skillStaffingDatas)
			{
				if (!staffingDataHasValue(skillStaffingData)) continue;

				if (satisfyAllSkills)
				{
					if (hasFairPossibilityInThisInterval(intervalPossibilities, skillStaffingData.Time))
						continue;
				}
				else
				{
					if (hasGoodPossibilityInThisInterval(intervalPossibilities, skillStaffingData.Time))
					{
						continue;
					}
				}

				var possibility = calculatePossibility(skillStaffingData);
				var key = skillStaffingData.Time;
				intervalPossibilities[key] = possibility;
			}
			return intervalPossibilities;
		}

		private static int calculatePossibility(SkillStaffingData skillStaffingData)
		{
			var isSatisfied = new IntervalHasSeriousUnderstaffing(skillStaffingData.Skill).IsSatisfiedBy(skillStaffingData.SkillStaffingInterval);
			return isSatisfied ? ScheduleStaffingPossibilityConsts.GoodPossibility : ScheduleStaffingPossibilityConsts.FairPossibility;
		}

		private static bool hasFairPossibilityInThisInterval(Dictionary<DateTime, int> intervalPossibilities, DateTime time)
		{
			return intervalPossibilities.TryGetValue(time, out var possibility) && possibility == ScheduleStaffingPossibilityConsts.FairPossibility;
		}

		private static bool hasGoodPossibilityInThisInterval(Dictionary<DateTime, int> intervalPossibilities, DateTime time)
		{
			return intervalPossibilities.TryGetValue(time, out var possibility) && possibility == ScheduleStaffingPossibilityConsts.GoodPossibility;
		}

		private static bool staffingDataHasValue(SkillStaffingData skillStaffingData)
		{
			var isScheduledStaffingDataAvailable = skillStaffingData.ScheduledStaffing.HasValue;
			var isForecastedStaffingDataAvailable = skillStaffingData.ForecastedStaffing.HasValue;
			return isScheduledStaffingDataAvailable && isForecastedStaffingDataAvailable;
		}
	}
}