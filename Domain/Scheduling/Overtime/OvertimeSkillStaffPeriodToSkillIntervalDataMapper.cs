using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IOvertimeSkillStaffPeriodToSkillIntervalDataMapper
    {
        IList<IOvertimeSkillIntervalData> MapSkillIntervalData(IEnumerable<ISkillStaffPeriod> skillStaffPeriod);
    }

    public class OvertimeSkillStaffPeriodToSkillIntervalDataMapper : IOvertimeSkillStaffPeriodToSkillIntervalDataMapper
    {
        private readonly IMergeOvertimeSkillIntervalData _mergeOvertimeSkillIntervalData;

        public OvertimeSkillStaffPeriodToSkillIntervalDataMapper(IMergeOvertimeSkillIntervalData mergeOvertimeSkillIntervalData)
        {
            _mergeOvertimeSkillIntervalData = mergeOvertimeSkillIntervalData;
        }

        public IList<IOvertimeSkillIntervalData> MapSkillIntervalData(IEnumerable<ISkillStaffPeriod> skillStaffPeriodList)
        {
            var skillIntervalList = new List<IOvertimeSkillIntervalData>();
            if (skillStaffPeriodList == null) return skillIntervalList;
            foreach (var skillStaffPeriod in skillStaffPeriodList)
            {
                var overtimeSkillIntervalData = skillIntervalList.FirstOrDefault(x => x.Period.Equals(skillStaffPeriod.Period));
                if (overtimeSkillIntervalData == null)
                {
                    skillIntervalList.Add(new OvertimeSkillIntervalData(skillStaffPeriod.Period, skillStaffPeriod.FStaff, skillStaffPeriod.FStaff - skillStaffPeriod.CalculatedResource));
                }
                else
                {
                    skillIntervalList.Remove(overtimeSkillIntervalData);
                    var overtimeSkillIntervalData2 = new OvertimeSkillIntervalData(skillStaffPeriod.Period, skillStaffPeriod.FStaff, skillStaffPeriod.FStaff - skillStaffPeriod.CalculatedResource);
                    overtimeSkillIntervalData = _mergeOvertimeSkillIntervalData.MergeSkillIntervalData(overtimeSkillIntervalData,
                                                                           overtimeSkillIntervalData2);
                    skillIntervalList.Add(overtimeSkillIntervalData);
                }
            }
            return skillIntervalList;
        }

    }
}