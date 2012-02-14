using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public interface IQuickForecastView
    {
        void SetWorkloadCollection(IEnumerable<WorkloadModel> workloadModels);
        void SetScenarioCollection(IEnumerable<ScenarioModel> scenarioModels);
        void SetSelectedScenario(ScenarioModel scenario);
        void SetStatisticPeriod(DateOnlyPeriod statisticPeriod);
        void SetTargetPeriod(DateOnlyPeriod targetPeriod);
        void AppendWorkInProgress(string status);
    }
}