using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class OvertimeStaffingPossibilityCalculator : IOvertimeStaffingPossibilityCalculator
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ISkillStaffingDataLoader _skillStaffingDataLoader;
		private readonly IOvertimeRequestSkillProvider _overtimeRequestSkillProvider;
		private readonly ISkillStaffingDataSkillTypeFilter _skillStaffingDataSkillTypeFilter;

		public OvertimeStaffingPossibilityCalculator(ILoggedOnUser loggedOnUser,
			ISkillStaffingDataLoader skillStaffingDataLoader,
			IOvertimeRequestSkillProvider overtimeRequestSkillProvider, ISkillStaffingDataSkillTypeFilter skillStaffingDataSkillTypeFilter)
		{
			_loggedOnUser = loggedOnUser;
			_skillStaffingDataLoader = skillStaffingDataLoader;
			_overtimeRequestSkillProvider = overtimeRequestSkillProvider;
			_skillStaffingDataSkillTypeFilter = skillStaffingDataSkillTypeFilter;
		}

		public IList<CalculatedPossibilityModel> CalculateIntradayIntervalPossibilities(DateOnlyPeriod period, bool satisfyAllSkills)
		{
			var person = _loggedOnUser.CurrentUser();
			var defaultTimeZone = person.PermissionInformation.DefaultTimeZone();

			var skills = new List<ISkill>();
			foreach (var date in period.DayCollection())
			{
				skills.AddRange(_overtimeRequestSkillProvider.GetAvailableSkillsBySkillType(person, date.ToDateTimePeriod(defaultTimeZone)).ToList());
			}

			var useShrinkage = person.WorkflowControlSet.OvertimeRequestStaffingCheckMethod ==
							OvertimeRequestStaffingCheckMethod.IntradayWithShrinkage;

			var skillStaffingDatas = _skillStaffingDataLoader.Load(skills.Distinct().ToList(), period, useShrinkage, isSiteOpened);

			var filteredSkillStaffingDatas = _skillStaffingDataSkillTypeFilter.Filter(skillStaffingDatas);

			return calculatePossibilities(filteredSkillStaffingDatas, satisfyAllSkills);
		}

		private bool isSiteOpened(DateOnly date)
		{
			var siteOpenHour = _loggedOnUser.CurrentUser().SiteOpenHour(date);
			if (siteOpenHour == null) return true;
			return !siteOpenHour.IsClosed;
		}

		private IList<CalculatedPossibilityModel> calculatePossibilities(
			IList<SkillStaffingData> skillStaffingDatas, bool satisfyAllSkills)
		{
			var resolution = skillStaffingDatas.FirstOrDefault()?.Resolution ?? 15;
			var calculatedPossibilityModels = new List<CalculatedPossibilityModel>();
			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Date);
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				calculatedPossibilityModels.Add(new CalculatedPossibilityModel
				{
					Date = skillStaffingDataGroup.Key,
					IntervalPossibilies = calculateIntervalPossibilities(skillStaffingDataGroup, satisfyAllSkills),
					Resolution = resolution
				});
			}
			return calculatedPossibilityModels;
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
			var isSatisfied = !new IntervalHasSeriousUnderstaffing(skillStaffingData.Skill).IsSatisfiedBy(skillStaffingData.SkillStaffingInterval);
			return isSatisfied ? ScheduleStaffingPossibilityConsts.FairPossibility : ScheduleStaffingPossibilityConsts.GoodPossibility;
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