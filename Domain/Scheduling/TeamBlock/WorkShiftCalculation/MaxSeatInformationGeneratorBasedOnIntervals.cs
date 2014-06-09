using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IMaxSeatInformationGeneratorBasedOnIntervals
	{
		IDictionary<DateTime, IntervalLevelMaxSeatInfo> GetMaxSeatInfo(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingResultStateHolder schedulingResultStateHolder, TimeZoneInfo timeZone);
	}

	public class MaxSeatInformationGeneratorBasedOnIntervals : IMaxSeatInformationGeneratorBasedOnIntervals
	{
		private readonly IMaxSeatSkillAggregator _maxSeatSkillAggregator;
		private readonly IMaxSeatsSpecificationDictionaryExtractor _maxSeatsSpecificationDictionaryExtractor;

		public MaxSeatInformationGeneratorBasedOnIntervals(IMaxSeatSkillAggregator maxSeatSkillAggregator, IMaxSeatsSpecificationDictionaryExtractor maxSeatsSpecificationDictionaryExtractor)
		{
			_maxSeatSkillAggregator = maxSeatSkillAggregator;
			_maxSeatsSpecificationDictionaryExtractor = maxSeatsSpecificationDictionaryExtractor;
		}

		public IDictionary<DateTime, IntervalLevelMaxSeatInfo> GetMaxSeatInfo(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingResultStateHolder schedulingResultStateHolder, TimeZoneInfo timeZone)
		{
			var skills = _maxSeatSkillAggregator.GetAggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers.ToList(), new DateOnlyPeriod(datePointer,datePointer ) ).ToList();
			var skillDaysOnDatePointer = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { datePointer });
			var skillDaysHavingMaxSeatInfo = new List<ISkillDay>();
			foreach (var skillDay in skillDaysOnDatePointer)
			{
				if (skills.Contains(skillDay.Skill))
					skillDaysHavingMaxSeatInfo.Add(skillDay);
			}
			IDictionary<DateTime, IntervalLevelMaxSeatInfo> maxSeatInfoOnEachIntervalDic = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			if (skillDaysHavingMaxSeatInfo.Any( ))
			{
				var skillStaffPeriodCollection = skillDaysHavingMaxSeatInfo[0].SkillStaffPeriodCollection;
				maxSeatInfoOnEachIntervalDic =  _maxSeatsSpecificationDictionaryExtractor.ExtractMaxSeatsFlag(skillStaffPeriodCollection.ToList() , timeZone  );
			}
			return maxSeatInfoOnEachIntervalDic;
		}
	}
}
