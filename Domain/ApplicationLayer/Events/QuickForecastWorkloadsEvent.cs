using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
    /// <summary>
    /// Message with details to perform an quick forecast on a workload.
    /// </summary>
    public class QuickForecastWorkloadsEvent : IEvent, ILogOnContext
    {
        /// <summary>
        /// The Job ID
        /// </summary>
        public Guid Identity
        {
            get { return JobId; }
        }

        /// <summary>
        /// The workloads to recalculate
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<Guid> WorkloadIds { get; set; }

        /// <summary>
        /// An id identifying this job
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// The Id of the scenario to forecast
        /// </summary>
        public Guid ScenarioId { get; set; }

        /// <summary>
        /// The period to base the forecast on
        /// </summary>
        public DateTime StatisticPeriodStart { get; set; }
        public DateTime StatisticPeriodEnd { get; set; }

        /// <summary>
        /// The period to forecast
        /// </summary>
        public DateTime TargetPeriodStart { get; set; }
        public DateTime TargetPeriodEnd { get; set; }

        /// <summary>
        /// The smoothing style of the templates
        /// </summary>
        public int SmoothingStyle { get; set; }

        /// <summary>
        /// The period to get the templates from
        /// </summary>
        public DateTime TemplatePeriodStart { get; set; }
        public DateTime TemplatePeriodEnd { get; set; }

        /// <summary>
        /// How much the progress bar should increase for every step
        /// </summary>
        public int IncreaseWith { get; set; }

        /// <summary>
        /// If the Index of the day of month should be used when forecasting
        /// </summary>
        public bool UseDayOfMonth { get; set; }

        public string LogOnDatasource { get; set; }
        public Guid LogOnBusinessUnitId { get; set; }
    }
}