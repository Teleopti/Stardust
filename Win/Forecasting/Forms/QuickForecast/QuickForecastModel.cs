using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public class QuickForecastModel
    {
        public QuickForecastModel()
        {
            SelectedWorkloads = new HashSet<WorkloadModel>();
        }

        public IScenario Scenario { get; set; }

        public ICollection<WorkloadModel> SelectedWorkloads { get; private set; }

        public DateOnlyPeriod StatisticPeriod { get; set; }

        public DateOnlyPeriod TargetPeriod { get; set; }

        public bool UpdateStandardTemplates { get; set; }
    }
}