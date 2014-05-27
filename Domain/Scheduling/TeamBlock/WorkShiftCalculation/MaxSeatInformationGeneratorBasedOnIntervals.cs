using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IMaxSeatInformationGeneratorBasedOnIntervals
	{
		IDictionary<DateTime, bool> GetMaxSeatInfo(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingResultStateHolder  schedulingResultStateHolder);
	}

	public class MaxSeatInformationGeneratorBasedOnIntervals : IMaxSeatInformationGeneratorBasedOnIntervals
	{
		private readonly IMaxSeatSkillAggregator _maxSeatSkillAggregator;

		public MaxSeatInformationGeneratorBasedOnIntervals(IMaxSeatSkillAggregator maxSeatSkillAggregator)
		{
			_maxSeatSkillAggregator = maxSeatSkillAggregator;
		}

		public IDictionary<DateTime, bool> GetMaxSeatInfo(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingResultStateHolder  schedulingResultStateHolder)
		{
			var skills = _maxSeatSkillAggregator.GetAggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers.ToList(), datePointer ).ToList();
			var skillDaysOnDatePointer = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { datePointer });
			var skillDaysHavingMaxSeatInfo = new List<ISkillDay>();
			foreach (var skillDay in skillDaysOnDatePointer)
			{
				if (skills.Contains(skillDay.Skill))
					skillDaysHavingMaxSeatInfo.Add(skillDay);
			}
			//is this check really valid?
			var maxSeatInfoOnEachIntervalDic = new Dictionary<DateTime, bool>();
			if (skillDaysHavingMaxSeatInfo.Any( ))
			{
				//skillStaffPeriodCollection = skillDaysHavingMaxSeatInfo[0].SkillStaffPeriodCollection;
				//call Jovi code here and return the result
			}
			//is this really valid
			return maxSeatInfoOnEachIntervalDic;
		}
	}
}
