using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public interface IQuickForecastScenarioProvider
    {
        IScenario DefaultScenario { get; }
        IEnumerable<IScenario> AllScenarios { get; }
    }
}