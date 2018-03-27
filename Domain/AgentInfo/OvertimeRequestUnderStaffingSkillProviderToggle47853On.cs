﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

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

		public IList<ISkill> GetSeriousUnderstaffingSkills(DateTimePeriod dateTimePeriod, IEnumerable<ISkill> skills, TimeZoneInfo timeZoneInfo)
		{
			var resolution = skills.Min(s => s.DefaultResolution);
			dateTimePeriod = convertToClosestPeriod(dateTimePeriod, resolution);
			var skillStaffingDatas = _skillStaffingDataLoader.Load(skills.ToList(), dateTimePeriod.ToDateOnlyPeriod(timeZoneInfo), true);
			skillStaffingDatas = skillStaffingDatas.Where(x =>
					x.Time >= dateTimePeriod.StartDateTimeLocal(timeZoneInfo) &&
					x.Time.AddMinutes(x.Resolution) <= dateTimePeriod.EndDateTimeLocal(timeZoneInfo)).ToList();
			
			if (!skillStaffingDatas.Any())
				return new ISkill[] { };

			skillStaffingDatas.ForEach(y => y.SkillStaffingInterval = new SkillStaffingInterval
			{
				CalculatedResource = y.ScheduledStaffing.GetValueOrDefault(),
				FStaff = y.ForecastedStaffing.GetValueOrDefault()
			});

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

			if (_overtimeRequestCriticalUnderStaffedSpecification.IsSatisfiedBy(new OvertimeRequestValidatedSkillCount(seriousUnderstaffingSkills.Count, skillStaffingDataGroups.Count)))
			{
				return new [] { mostUnderStaffedSkill };
			}

			return new ISkill [] { };
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