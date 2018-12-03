using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	[EnabledBy(Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
	public class OvertimeRequestUnderStaffingSkillProviderToggle47853On : IOvertimeRequestUnderStaffingSkillProvider
	{
		private readonly ISkillStaffingDataLoader _skillStaffingDataLoader;
		private readonly IOvertimeRequestCriticalUnderStaffedSpecification _overtimeRequestCriticalUnderStaffedSpecification;

		public OvertimeRequestUnderStaffingSkillProviderToggle47853On(ISkillStaffingDataLoader skillStaffingDataLoader, IOvertimeRequestCriticalUnderStaffedSpecification overtimeRequestCriticalUnderStaffedSpecification)
		{
			_skillStaffingDataLoader = skillStaffingDataLoader;
			_overtimeRequestCriticalUnderStaffedSpecification = overtimeRequestCriticalUnderStaffedSpecification;
		}

		public IDictionary<DateTimePeriod,IList<ISkill>> GetSeriousUnderstaffingSkills(DateTimePeriod dateTimePeriod,
			IEnumerable<ISkill> skills, IPerson person)
		{
			var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
			var useShrinkage = person.WorkflowControlSet.OvertimeRequestStaffingCheckMethod ==
							   OvertimeRequestStaffingCheckMethod.IntradayWithShrinkage;

			var resolution = skills.Min(s => s.DefaultResolution);
			dateTimePeriod = convertToClosestPeriod(dateTimePeriod, resolution);
			var skillStaffingDatas =
				_skillStaffingDataLoader.Load(skills.ToList(), dateTimePeriod.ToDateOnlyPeriod(timeZoneInfo), useShrinkage);
			skillStaffingDatas = skillStaffingDatas.Where(x =>
				x.Time >= dateTimePeriod.StartDateTimeLocal(timeZoneInfo) &&
				x.Time.AddMinutes(x.Resolution) <= dateTimePeriod.EndDateTimeLocal(timeZoneInfo)).ToList();

			if (!skillStaffingDatas.Any())
				return new Dictionary<DateTimePeriod,IList<ISkill>>();
			
			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Skill).ToList();
			var seriousUnderstaffingSkills = new List<ISkill>();
			var minmumUnderStaffedLevel = double.MaxValue;
			ISkill mostUnderStaffedSkill = null;
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				var skillStaffingDataInPeriod = skillStaffingDataGroup.ToList();

				if (!skillStaffingDataInPeriod.Any())
					continue;

				if (skillStaffingDataInPeriod.Any(s => !hasSeriousUnderstaffing(skillStaffingDataGroup.Key, s)))
					continue;

				var skillUnderStaffedLevel = skillStaffingDataInPeriod.Sum(y => y.SkillStaffingInterval.RelativeDifference);
				if (skillUnderStaffedLevel < minmumUnderStaffedLevel)
				{
					minmumUnderStaffedLevel = skillUnderStaffedLevel;
					mostUnderStaffedSkill = skillStaffingDataGroup.Key;
				}

				seriousUnderstaffingSkills.Add(skillStaffingDataGroup.Key);
			}

			if (_overtimeRequestCriticalUnderStaffedSpecification.IsSatisfiedBy(
				new OvertimeRequestValidatedSkillCount(seriousUnderstaffingSkills.Count, skillStaffingDataGroups.Count)))
			{
				return new Dictionary<DateTimePeriod,IList<ISkill>>
				{
					{dateTimePeriod, new List<ISkill> {mostUnderStaffedSkill}}
				};
			}

			return new Dictionary<DateTimePeriod,IList<ISkill>>();
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
	}
}