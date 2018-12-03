using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class OvertimeRequestUnderStaffingSkillProviderToggle74944On : IOvertimeRequestUnderStaffingSkillProvider
	{
		private readonly ISkillStaffingDataLoader _skillStaffingDataLoader;

		public OvertimeRequestUnderStaffingSkillProviderToggle74944On(ISkillStaffingDataLoader skillStaffingDataLoader)
		{
			_skillStaffingDataLoader = skillStaffingDataLoader;
		}

		public IDictionary<DateTimePeriod, IList<ISkill>> GetSeriousUnderstaffingSkills(DateTimePeriod requestDateTimePeriod,
			IEnumerable<ISkill> skills, IPerson person)
		{
			var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
			var useShrinkage = person.WorkflowControlSet.OvertimeRequestStaffingCheckMethod ==
							   OvertimeRequestStaffingCheckMethod.IntradayWithShrinkage;

			var resolution = skills.Min(s => s.DefaultResolution);
			var dateTimePeriod = convertToClosestPeriod(requestDateTimePeriod, resolution);

			var skillStaffingDatas = loadSkillStaffingData(dateTimePeriod, skills, timeZoneInfo, useShrinkage);

			if (!skillStaffingDatas.Any())
				return new Dictionary<DateTimePeriod, IList<ISkill>>();

			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Skill).ToList();
			var skillStaffLevelDictionary = new Dictionary<DateTimePeriod, IList<SkillStaffLevel>>();
			var hasCriticalUnderStaffedSkillDictionary = dateTimePeriod.Intervals(TimeSpan.FromMinutes(resolution))
				.ToDictionary(x => x, x => false);

			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				var skillStaffingDataInPeriod = skillStaffingDataGroup.ToArray();
				var skill = skillStaffingDataGroup.Key;
				if (!skillStaffingDataInPeriod.Any())
					continue;

				foreach (var skillStaffingData in skillStaffingDataInPeriod)
				{
					var intervalUtc = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(skillStaffingData.Time, timeZoneInfo),
						TimeZoneHelper.ConvertToUtc(skillStaffingData.Time.AddMinutes(skillStaffingData.Resolution), timeZoneInfo));

					if (!hasSeriousUnderstaffing(skill, skillStaffingData)) continue;

					hasCriticalUnderStaffedSkillDictionary[intervalUtc] = true;

					if (!skillStaffLevelDictionary.TryGetValue(intervalUtc, out var staffingLevelList))
					{
						staffingLevelList = new List<SkillStaffLevel>();
						skillStaffLevelDictionary.Add(intervalUtc, staffingLevelList);
					}

					staffingLevelList.Add(new SkillStaffLevel
					{
						Skill = skillStaffingData.Skill,
						StaffLevel = skillStaffingData.SkillStaffingInterval.RelativeDifference
					});
				}
			}

			var hasAtLeastOneCriticalUnderstaffSkillInAllPeriods = hasCriticalUnderStaffedSkillDictionary.All(x => x.Value);
			if (!hasAtLeastOneCriticalUnderstaffSkillInAllPeriods)
			{
				return new Dictionary<DateTimePeriod, IList<ISkill>>();
			}

			return mergePeriodsWithSameSkill(skillStaffLevelDictionary, requestDateTimePeriod);
		}

		private IList<SkillStaffingData> loadSkillStaffingData(DateTimePeriod dateTimePeriod, IEnumerable<ISkill> skills, TimeZoneInfo timeZoneInfo, bool useShrinkage)
		{
			var skillStaffingDatas =
				_skillStaffingDataLoader.Load(skills.ToList(), dateTimePeriod.ToDateOnlyPeriod(timeZoneInfo), useShrinkage);

			skillStaffingDatas = skillStaffingDatas.Where(x =>
				x.Time >= dateTimePeriod.StartDateTimeLocal(timeZoneInfo) &&
				x.Time.AddMinutes(x.Resolution) <= dateTimePeriod.EndDateTimeLocal(timeZoneInfo)).ToList();

			return skillStaffingDatas;
		}

		private Dictionary<DateTimePeriod, IList<ISkill>> mergePeriodsWithSameSkill(Dictionary<DateTimePeriod, IList<SkillStaffLevel>> skillStaffLevelDictionary, DateTimePeriod periodRange)
		{
			var periods = skillStaffLevelDictionary.Keys.OrderBy(x => x.StartDateTime).ToList();
			if (!periods.Any())
				return new Dictionary<DateTimePeriod, IList<ISkill>>();

			DateTimePeriod? lastPeriod = null;
			var result = new Dictionary<DateTimePeriod, IList<ISkill>>();

			foreach (var period in periods)
			{
				var currentSkill = skillStaffLevelDictionary[period].OrderBy(s => s.StaffLevel).First().Skill;
				var currentPeriod = adjustStaffingPeriod(period, periodRange);

				if (lastPeriod.HasValue)
				{
					if (currentPeriod.StartDateTime.Equals(lastPeriod.Value.EndDateTime)
						&& result[lastPeriod.Value].First().Equals(currentSkill))
					{
						result.Remove(lastPeriod.Value);

						lastPeriod = new DateTimePeriod(lastPeriod.Value.StartDateTime, currentPeriod.EndDateTime);
						result.Add(lastPeriod.Value, new List<ISkill> { currentSkill });
					}
					else
					{
						result.Add(currentPeriod, new List<ISkill> { currentSkill });
						lastPeriod = currentPeriod;
					}
				}
				else
				{
					result.Add(currentPeriod, new List<ISkill> { currentSkill });
					lastPeriod = currentPeriod;
				}
			}

			return result;
		}

		private DateTimePeriod adjustStaffingPeriod(DateTimePeriod currentPeriod, DateTimePeriod periodRange)
		{
			if (currentPeriod.StartDateTime < periodRange.StartDateTime)
			{
				currentPeriod = new DateTimePeriod(periodRange.StartDateTime, currentPeriod.EndDateTime);
			}

			if (currentPeriod.EndDateTime > periodRange.EndDateTime)
			{
				currentPeriod = new DateTimePeriod(currentPeriod.StartDateTime, periodRange.EndDateTime);
			}

			return currentPeriod;
		}

		private bool hasSeriousUnderstaffing(ISkill skill, SkillStaffingData skillStaffingData)
		{
			return new IntervalHasSeriousUnderstaffing(skill).IsSatisfiedBy(skillStaffingData.SkillStaffingInterval);
		}

		private DateTimePeriod convertToClosestPeriod(DateTimePeriod dateTimePeriod, int resolution)
		{
			var startTimeLeftMinutes = dateTimePeriod.StartDateTime.Minute % resolution;
			if (startTimeLeftMinutes > 0)
			{
				dateTimePeriod = new DateTimePeriod(
					dateTimePeriod.StartDateTime.Subtract(TimeSpan.FromMinutes(startTimeLeftMinutes))
					, dateTimePeriod.EndDateTime);
			}

			var endTimeLeftMinutes = dateTimePeriod.EndDateTime.Minute % resolution;
			if (endTimeLeftMinutes > 0)
			{
				var minutesToNextInterval = resolution - endTimeLeftMinutes;
				dateTimePeriod = dateTimePeriod.ChangeEndTime(TimeSpan.FromMinutes(minutesToNextInterval));
			}

			return dateTimePeriod;
		}

		private class SkillStaffLevel
		{
			public ISkill Skill { get; set; }
			public double StaffLevel { get; set; }

		}
	}
}