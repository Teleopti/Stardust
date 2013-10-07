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
		private readonly IOvertimeSkillStaffPeriodToSkillIntervalDataMapper _overtimeSkillStaffPeriodToSkillIntervalDataMapper;
		private readonly IOvertimeSkillIntervalDataDivider _overtimeSkillIntervalDataDivider;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

        public OvertimeLengthDecider(ISkillResolutionProvider skillResolutionProvider,
		                             IOvertimeSkillStaffPeriodToSkillIntervalDataMapper overtimeSkillStaffPeriodToSkillIntervalDataMapper,
		                             IOvertimeSkillIntervalDataDivider overtimeSkillIntervalDataDivider,
		                             ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_skillResolutionProvider = skillResolutionProvider;
			_overtimeSkillStaffPeriodToSkillIntervalDataMapper = overtimeSkillStaffPeriodToSkillIntervalDataMapper;
			_overtimeSkillIntervalDataDivider = overtimeSkillIntervalDataDivider;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public TimeSpan Decide(IPerson person, DateOnly dateOnly, DateTime overtimeStartTime, IActivity activity, MinMax<TimeSpan> duration)
		{
			var skills = aggregateSkills(person, dateOnly).Where(x=>x.Activity == activity).ToList();
			if (skills.Count == 0) return TimeSpan.Zero;
			var minimumResolution = _skillResolutionProvider.MinimumResolution(skills);
			var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {dateOnly});
			if (skillDays == null) return TimeSpan.Zero;
			var mappedData = _overtimeSkillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillDays.SelectMany(x=>x.SkillStaffPeriodCollection));
			mappedData = _overtimeSkillIntervalDataDivider.SplitSkillIntervalData(mappedData, minimumResolution);
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
			if (durationValuePairs.Count == 0)
				return TimeSpan.Zero;
			var worstRelativeDifference = durationValuePairs.Min(x => x.Value);
			if (worstRelativeDifference >= 0 || double.IsNaN(worstRelativeDifference)) return TimeSpan.Zero;
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