using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IOpenHourRestrictionForTeamBlock
    {
        IDictionary<IActivity, TimePeriod> GetOpenHoursPerActivity(ITeamBlockInfo teamBlock);
        bool HasSameOpeningHours(ITeamBlockInfo teamBlock);
    }

    public class OpenHourRestrictionForTeamBlock : IOpenHourRestrictionForTeamBlock
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;
        private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;

        public OpenHourRestrictionForTeamBlock(ISchedulingResultStateHolder schedulingResultStateHolder,ISkillIntervalDataOpenHour skillIntervalDataOpenHour,ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper  )
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _skillIntervalDataOpenHour = skillIntervalDataOpenHour;
            _skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
        }

        public IDictionary<IActivity, TimePeriod> GetOpenHoursPerActivity(ITeamBlockInfo teamBlockInfo)
        {
            var dateOnlyList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
            var openHoursPerActivity = new Dictionary<IActivity, TimePeriod>();
            foreach (var skillDay in skillDays)
            {
                var activity = skillDay.Skill.Activity;
                if (activity == null) continue;
                if (skillDay.SkillStaffPeriodCollection.Count == 0) continue;
                var openHourForSkillDay = _skillIntervalDataOpenHour.GetOpenHours(_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillDay.SkillStaffPeriodCollection));
                if (!openHoursPerActivity.ContainsKey(activity))
                {
                    openHoursPerActivity.Add(activity,openHourForSkillDay );
                }
                else
                {
                    openHoursPerActivity[activity] = getNarrowTimePeriod(openHoursPerActivity[activity], openHourForSkillDay);
                }

            }
            return openHoursPerActivity;
        }

        public bool HasSameOpeningHours(ITeamBlockInfo teamBlockInfo)
        {
            var dateOnlyList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
            if (skillDays.Count > 0)
            {
                var sampleHour = getSampleHour(skillDays);
                if (sampleHour.HasValue)
                {
                    foreach (var skillDay in skillDays)
                    {
                        if (skillDay.SkillStaffPeriodCollection.Count == 0) continue;
                        var openHourForSkillDay =
                            _skillIntervalDataOpenHour.GetOpenHours(
                                _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(
                                    skillDay.SkillStaffPeriodCollection));
                        if (sampleHour != openHourForSkillDay)
                            return false;
                    }
                }
                
            }

            return true;
        }

        private TimePeriod? getSampleHour(IList<ISkillDay  > skillDays)
        {
            foreach (var skillDay in skillDays)
            {
                if (skillDay.SkillStaffPeriodCollection.Count > 0)
                {
                    return _skillIntervalDataOpenHour.GetOpenHours(
                        _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(
                            skillDay.SkillStaffPeriodCollection));
                }
            }
            return null;
        }

        private TimePeriod getNarrowTimePeriod(TimePeriod existingTimePeriod, TimePeriod newTimePeriod)
        {
			if (existingTimePeriod.EndTime.Days > 0 && newTimePeriod.EndTime.Days == 0)
			{
				var temp = new TimePeriod(newTimePeriod.StartTime.Add(TimeSpan.FromDays(1)),
										  newTimePeriod.EndTime.Add(TimeSpan.FromDays(1)));

				var startTime = existingTimePeriod.StartTime;
				var endTime = existingTimePeriod.EndTime;
				if (existingTimePeriod.StartTime < temp.StartTime)
					startTime = temp.StartTime.Add(TimeSpan.FromDays(-1));
				if (existingTimePeriod.EndTime > temp.EndTime)
					endTime = temp.EndTime.Add(TimeSpan.FromDays(-1));
				return new TimePeriod(startTime, endTime);
			}
			if (newTimePeriod.EndTime.Days > 0 && existingTimePeriod.EndTime.Days == 0)
			{
				var temp = new TimePeriod(existingTimePeriod.StartTime.Add(TimeSpan.FromDays(1)),
										  existingTimePeriod.EndTime.Add(TimeSpan.FromDays(1)));
				var startTime = newTimePeriod.StartTime;
				var endTime = newTimePeriod.EndTime;
				if (temp.StartTime > newTimePeriod.StartTime)
					startTime = temp.StartTime.Add(TimeSpan.FromDays(-1));
				if (temp.EndTime < newTimePeriod.EndTime)
					endTime = temp.EndTime.Add(TimeSpan.FromDays(-1));
				return new TimePeriod(startTime, endTime);
			}
			else
			{
				var startTime = existingTimePeriod.StartTime;
				var endTime = existingTimePeriod.EndTime;
				if (existingTimePeriod.StartTime < newTimePeriod.StartTime)
					startTime = newTimePeriod.StartTime;
				if (existingTimePeriod.EndTime > newTimePeriod.EndTime)
					endTime = newTimePeriod.EndTime;
				return new TimePeriod(startTime, endTime);
			}

        }
        
    }
}
