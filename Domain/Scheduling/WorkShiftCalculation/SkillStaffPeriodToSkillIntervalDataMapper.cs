using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface ISkillStaffPeriodToSkillIntervalDataMapper
    {
        IList<ISkillIntervalData> MapSkillIntervalData(IList<ISkillStaffPeriod> skillStaffPeriod);
    }

    public class SkillStaffPeriodToSkillIntervalDataMapper : ISkillStaffPeriodToSkillIntervalDataMapper
    {
        public IList<ISkillIntervalData> MapSkillIntervalData(IList<ISkillStaffPeriod> skillStaffPeriodList)
        {
            var skillIntervalList = new List<ISkillIntervalData>();
            foreach (var skillStaffPeriod in skillStaffPeriodList)
            {
                skillIntervalList.Add(new SkillIntervalData(skillStaffPeriod.Period ,skillStaffPeriod.FStaff ,skillStaffPeriod.FStaff - skillStaffPeriod.CalculatedResource ,0,null,null));
            }
            return skillIntervalList;
        }
    }
}