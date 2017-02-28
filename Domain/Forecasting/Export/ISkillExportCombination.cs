using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public interface ISkillExportCombination
    {
        IChildSkill SourceSkill { get; }
        ISkill TargetSkill { get; }
    }
}