using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class SkillExportCombination : ISkillExportCombination
    {
        public IChildSkill SourceSkill { get; set; }

        public ISkill TargetSkill { get; set; }
    }
}