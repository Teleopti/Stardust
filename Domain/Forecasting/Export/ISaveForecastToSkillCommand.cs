using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public interface ISaveForecastToSkillCommand
    {
        void Execute(DateOnlyPeriod period, ISkill targetSkill, ISkillStaffPeriodDictionary sourceSkillStaffPeriods);
        void Execute(DateOnly date, ISkill targetSkill, ICollection<IForecastsFileRow> forecasts, ImportForecastsMode importMode);
    }
}