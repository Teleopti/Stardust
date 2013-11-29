using System.Collections.Generic;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification
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
                var sampleHour = getSampleHour(skillDays);
                if (sampleHour.HasValue)
                {
                    foreach (var skillDay in skillDays)
                    {
						var skillStaffPeriodCollection = skillDay.SkillStaffPeriodCollection;
						if (skillStaffPeriodCollection.Count == 0)
							continue;
	                    var mappedSkillIntervalData =
		                    _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillStaffPeriodCollection);
                        var openHourForSkillDay =
							_skillIntervalDataOpenHour.GetOpenHours(mappedSkillIntervalData, skillDay.CurrentDate);
                        if (sampleHour != openHourForSkillDay)
                            return false;
                    }
                }
            }

            return true;
        }

        private TimePeriod? getSampleHour(IEnumerable<ISkillDay> skillDays)
        {
            foreach (var skillDay in skillDays)
            {
	            var skillStaffPeriodCollection = skillDay.SkillStaffPeriodCollection;
				var mappedSkillIntervalData =
							_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillStaffPeriodCollection);
				if (skillStaffPeriodCollection.Count > 0)
                {
					var openHourPeriod = _skillIntervalDataOpenHour.GetOpenHours(mappedSkillIntervalData, skillDay.CurrentDate);

	                return openHourPeriod;
                }
            }
            return null;
        }
    }
}