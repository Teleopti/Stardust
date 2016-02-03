using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public interface ISaveForecastToSkillCommand
    {
        void Execute(DateOnly dateOnly, ISkill targetSkill, ICollection<IForecastsRow> forecasts, ImportForecastsMode importMode);
    }
}