using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class SkillExportCombination
    {
        public IChildSkill SourceSkill { get; set; }

        public ISkill TargetSkill { get; set; }
    }
}