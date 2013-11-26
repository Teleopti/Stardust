using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
    public interface IOpenHourRestrictionForTeamBlock
    {
        IDictionary<IActivity, TimePeriod> GetOpenHoursPerActivity(ITeamBlockInfo teamBlock);
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
	            var openHourForSkillDay =
		            _skillIntervalDataOpenHour.GetOpenHours(
			            _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillDay.SkillStaffPeriodCollection),
			            skillDay.CurrentDate);
                if (!openHoursPerActivity.ContainsKey(activity))
                {
                    openHoursPerActivity.Add(activity, openHourForSkillDay);
                }
                else
                {
                    openHoursPerActivity[activity] = getNarrowTimePeriod(openHoursPerActivity[activity], openHourForSkillDay);
                }

            }
            return openHoursPerActivity;
        }

        private TimePeriod getNarrowTimePeriod(TimePeriod existingTimePeriod, TimePeriod newTimePeriod)
        {
            if (existingTimePeriod.EndTime.Days > 0 && newTimePeriod.EndTime.Days == 0)
            {
                var temp = new TimePeriod(newTimePeriod.StartTime.Add(TimeSpan.FromDays(1)),
                                          newTimePeriod.EndTime.Add(TimeSpan.FromDays(1)));

                var extractedTimePeriod = getFinalTimePeriod(existingTimePeriod, temp);
                return new TimePeriod(extractedTimePeriod.StartTime.Add(TimeSpan.FromDays(-1)), extractedTimePeriod.EndTime.Add(TimeSpan.FromDays(-1)));
            }
            if (newTimePeriod.EndTime.Days > 0 && existingTimePeriod.EndTime.Days == 0)
            {
                var temp = new TimePeriod(existingTimePeriod.StartTime.Add(TimeSpan.FromDays(1)),
                                          existingTimePeriod.EndTime.Add(TimeSpan.FromDays(1)));
                var extractedTimePeriod = getFinalTimePeriod(newTimePeriod, temp);
                return new TimePeriod(extractedTimePeriod.StartTime.Add(TimeSpan.FromDays(-1)), extractedTimePeriod.EndTime.Add(TimeSpan.FromDays(-1)));
            }
            else
            {
                var extractedTimePeriod = getFinalTimePeriod(existingTimePeriod, newTimePeriod);
                return new TimePeriod(extractedTimePeriod.StartTime, extractedTimePeriod.EndTime);
            }

        }

        private TimePeriod getFinalTimePeriod(TimePeriod source, TimePeriod destination)
        {
            var startTime = source.StartTime;
            var endTime = source.EndTime;
            if (source.StartTime < destination.StartTime)
                startTime = destination.StartTime;
            if (source.EndTime > destination.EndTime)
                endTime = destination.EndTime;
            return new TimePeriod(startTime, endTime);
        }
        
    }



}
