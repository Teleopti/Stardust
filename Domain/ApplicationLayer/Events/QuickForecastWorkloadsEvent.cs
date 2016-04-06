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
        /// The period to base the forecast on, divided due to JSon on Hangfire
        /// </summary>
        public DateOnly StatisticsPeriodStart { get; set; }
        public DateOnly StatisticsPeriodEnd { get; set; }


        /// <summary>
        /// The period to forecast,  divided due to JSon on Hangfire
        /// </summary>
        public DateOnly TargetPeriodStart { get; set; }
        public DateOnly TargetPeriodEnd { get; set; }


        /// <summary>
        /// The smoothing style of the templates
        /// </summary>
        public int SmoothingStyle { get; set; }

        /// <summary>
        /// The period to get the templates from,  divided due to JSon on Hangfire
        /// </summary>
        public DateOnly TemplatePeriodStart { get; set; }
        public DateOnly TemplatePeriodEnd { get; set; }

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