using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class SkillStaffingReadModelDataLoader : ISkillStaffingReadModelDataLoader
	{
		private readonly SkillStaffingIntervalProvider _skillStaffingIntervalProvider;

		public SkillStaffingReadModelDataLoader(SkillStaffingIntervalProvider skillStaffingIntervalProvider)
		{
			_skillStaffingIntervalProvider = skillStaffingIntervalProvider;
		}

		public IList<SkillStaffingData> Load(IList<ISkill> skills, DateTimePeriod dateTimePeriod)
		{
			if (!skills.Any()) return new List<SkillStaffingData>();

			return createSkillStaffingDatas(dateTimePeriod, skills);
		}

		private IList<SkillStaffingData> createSkillStaffingDatas(DateTimePeriod period, IList<ISkill> skills)
		{
			var resolution = skills.Min(s => s.DefaultResolution);
			var skillDictionary = skills.Distinct().ToDictionary(s => s.Id.GetValueOrDefault());
			var skillIds = skillDictionary.Keys.ToArray();
			var skillStaffingIntervals = _skillStaffingIntervalProvider.StaffingIntervalsForSkills(skillIds, period,false);

			var skillStaffingDatas = new List<SkillStaffingData>();
			var skillStaffingIntervalGroups = skillStaffingIntervals.GroupBy(s => s.SkillId);

			foreach (var skillStaffingIntervalGroup in skillStaffingIntervalGroups)
			{
				skillStaffingDatas.AddRange(skillStaffingIntervalGroup.Select(skillStaffingInterval => new SkillStaffingData
				{
					Skill = skillDictionary[skillStaffingIntervalGroup.Key],
					ForecastedStaffing = skillStaffingInterval.FStaff,
					ScheduledStaffing = skillStaffingInterval.StaffingLevel,
					Time = skillStaffingInterval.StartDateTime,
					Date = new DateOnly(skillStaffingInterval.StartDateTime),
					Resolution = resolution
				}));
			}

			return skillStaffingDatas;
		}
	}
}
