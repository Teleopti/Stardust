using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public interface ISaveForecastToSkill
    {
        void Execute(DateOnly dateOnly, ISkill targetSkill, ICollection<ForecastsRow> forecasts, ImportForecastsMode importMode);
    }
}