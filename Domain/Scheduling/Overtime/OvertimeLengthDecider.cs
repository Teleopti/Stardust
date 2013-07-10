using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeLengthDecider
	{
		TimeSpan Decide(IPerson person, DateOnly dateOnly, DateTime overtimeStartTime, IActivity activity,
							 MinMax<TimeSpan> duration);
	}

	public class OvertimeLengthDecider : IOvertimeLengthDecider
	{
		private readonly ISkillResolutionProvider _skillResolutionProvider;
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataDivider _skillIntervalDataDivider;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public OvertimeLengthDecider(ISkillResolutionProvider skillResolutionProvider,
		                             ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
		                             ISkillIntervalDataDivider skillIntervalDataDivider,
		                             ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_skillResolutionProvider = skillResolutionProvider;
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataDivider = skillIntervalDataDivider;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public TimeSpan Decide(IPerson person, DateOnly dateOnly, DateTime overtimeStartTime, IActivity activity, MinMax<TimeSpan> duration)
		{
			var skills = aggregateSkills(person, dateOnly).Where(x=>x.Activity == activity).ToList();
			if (skills.Count == 0) return TimeSpan.Zero;
			var minimumResolution = _skillResolutionProvider.MinimumResolution(skills);
			var skillDay = _schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {dateOnly}).FirstOrDefault();
			if (skillDay == null) return TimeSpan.Zero;
			var mappedData = _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillDay.SkillStaffPeriodCollection);
			mappedData = _skillIntervalDataDivider.SplitSkillIntervalData(mappedData, minimumResolution);
			var durationValuePairs = new List<KeyValuePair<TimeSpan, double>>();
			var minEndTime = overtimeStartTime.Add(duration.Minimum);
			var maxEndTime = overtimeStartTime.Add(duration.Maximum);
			var overtimeEndTime = overtimeStartTime;
			while (overtimeEndTime < maxEndTime)
			{
				overtimeEndTime = overtimeEndTime.Add(TimeSpan.FromMinutes(minimumResolution));
				if(overtimeEndTime < minEndTime) continue;	
				var overtimePeriod = new DateTimePeriod(overtimeStartTime, overtimeEndTime);
				var sumOfRelativeDifference = mappedData.Where(x => overtimePeriod.Contains(x.Period))
				                                        .Sum(x => x.RelativeDifference);
				var pair = new KeyValuePair<TimeSpan, double>(overtimeEndTime-overtimeStartTime, sumOfRelativeDifference);
				durationValuePairs.Add(pair);
			}
			var worstRelativeDifference = durationValuePairs.Min(x => x.Value);
			if (worstRelativeDifference >= 0) return TimeSpan.Zero;
			return durationValuePairs.First(x => x.Value == worstRelativeDifference).Key;
		}

		private static IEnumerable<ISkill> aggregateSkills(IPerson person, DateOnly dateOnly)
		{
			var ret = new List<ISkill>();
			var personPeriod = person.Period(dateOnly);

			foreach (var personSkill in personPeriod.PersonSkillCollection)
			{
				if (!ret.Contains(personSkill.Skill))
					ret.Add(personSkill.Skill);
			}
			return ret;
		}
	}
}