using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public class FakePersonSkillProvider: IPersonSkillProvider
    {
        public SkillCombination SkillCombination { get; set; }

        public SkillCombination SkillsOnPersonDate(IPerson person, DateOnly date)
        {
            return SkillCombination;
        }
    }
}
