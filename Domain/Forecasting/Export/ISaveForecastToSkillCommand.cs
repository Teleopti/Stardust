using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public interface ISaveForecastToSkillCommand
    {
        void Execute(DateOnlyPeriod period, ISkill targetSkill, ISkillStaffPeriodDictionary sourceSkillStaffPeriods);
    }
}