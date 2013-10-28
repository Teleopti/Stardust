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
        
        private TimePeriod getNarrowTimePeriod(TimePeriod existingTimePeriod, TimePeriod newTimePeriod)
        {
            var startTime = existingTimePeriod.StartTime;
            var endTime = existingTimePeriod.EndTime;
            if (existingTimePeriod.StartTime < newTimePeriod.StartTime)
                startTime = newTimePeriod.StartTime;
            if (existingTimePeriod.EndTime > newTimePeriod.EndTime)
                endTime = newTimePeriod.EndTime;
            return new TimePeriod(startTime,endTime );
        }
        
    }
}
