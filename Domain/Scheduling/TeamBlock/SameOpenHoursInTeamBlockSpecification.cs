using System.Collections.Generic;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ISameOpenHoursInTeamBlockSpecification
    {
        bool IsSatisfiedBy(ITeamBlockInfo skillDays);
    }

    public class SameOpenHoursInTeamBlockSpecification : Specification<ITeamBlockInfo>, ISameOpenHoursInTeamBlockSpecification
    {
        private readonly ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;
        private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

        public SameOpenHoursInTeamBlockSpecification(ISkillIntervalDataOpenHour skillIntervalDataOpenHour, ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper, ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            _skillIntervalDataOpenHour = skillIntervalDataOpenHour;
            _skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }

        public override bool IsSatisfiedBy(ITeamBlockInfo  teamBlockInfo)
        {
            var dateOnlyList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
            if (skillDays.Count > 0)
            {
                if (skillDays[0].SkillStaffPeriodCollection.Count > 0)
                {
                    var sampleHour =
                    _skillIntervalDataOpenHour.GetOpenHours(
                        _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(
                            skillDays[0].SkillStaffPeriodCollection));
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
    }
}