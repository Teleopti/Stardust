using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class TemplateTaskPeriodView :ITemplateTaskPeriodView
    {
        public DateTimePeriod Period { get; set; }

        public TimeSpan AverageTaskTime { get; set; }

        public TimeSpan AverageAfterTaskTime { get; set; }

        public Percent CampaignTaskTime { get; set; }

        public Percent CampaignAfterTaskTime { get; set; }

        public TimeSpan TotalAverageTaskTime { get; set; }

        public TimeSpan TotalAverageAfterTaskTime { get; set; }

        public double Tasks { get; set; }

        public double CampaignTasks { get; set; }

        public double TotalTasks { get; set; }

        public IWorkloadDay Parent { get; set; }
    }
}
