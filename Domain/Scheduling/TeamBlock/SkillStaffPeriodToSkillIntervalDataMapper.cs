using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ISkillStaffPeriodToSkillIntervalDataMapper
    {
        IList<ISkillIntervalData> MapSkillIntervalData(IList<ISkillStaffPeriod> skillStaffPeriod);
    }

    public class SkillStaffPeriodToSkillIntervalDataMapper : ISkillStaffPeriodToSkillIntervalDataMapper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public IList<ISkillIntervalData> MapSkillIntervalData(IList<ISkillStaffPeriod> skillStaffPeriodList)
        {
            var skillIntervalList = new List<ISkillIntervalData>();
            if (skillStaffPeriodList != null)
                foreach (var skillStaffPeriod in skillStaffPeriodList)
                {
                    skillIntervalList.Add(new SkillIntervalData(skillStaffPeriod.Period ,skillStaffPeriod.FStaff ,skillStaffPeriod.FStaff - skillStaffPeriod.CalculatedResource ,0,null,null));
                }
            return skillIntervalList;
        }
    }
}