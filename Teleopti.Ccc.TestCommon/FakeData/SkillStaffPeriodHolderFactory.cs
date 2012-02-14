using System.Reflection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class SkillStaffPeriodHolderFactory
    {

        public static ISkillStaffPeriodHolder InjectSkillStaffPeriods(SkillStaffPeriodHolder skillStaffPeriodHolder, SkillSkillStaffPeriodExtendedDictionary skillStaffPeriods)
        {
            typeof(SkillStaffPeriodHolder).GetField("_internalDictionary", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(skillStaffPeriodHolder, skillStaffPeriods);
            return skillStaffPeriodHolder;
        }
    }
}
