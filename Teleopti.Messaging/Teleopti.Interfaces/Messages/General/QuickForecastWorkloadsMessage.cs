using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Messages.General
{
	/// <summary>
	/// Message with details to perform an quick forecast on a workload.
	/// </summary>
	public class QuickForecastWorkloadsMessage : MessageWithLogOnContext
	{
		/// <summary>
		/// The Job ID
		/// </summary>
		public override Guid Identity
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
		public DateOnlyPeriod StatisticPeriod { get; set; }

		/// <summary>
		/// The period to forecast
		/// </summary>
		public DateOnlyPeriod TargetPeriod { get; set; }

		/// <summary>
		/// The smoothing style of the templates
		/// </summary>
		public int SmoothingStyle { get; set; }

		/// <summary>
		/// The period to get the templates from
		/// </summary>
		public DateOnlyPeriod TemplatePeriod { get; set; }

		/// <summary>
		/// How much the progress bar should increase for every step
		/// </summary>
		public int IncreaseWith { get; set; }

        /// <summary>
        /// If the Index of the day of month should be used when forecasting
        /// </summary>
        public bool UseDayOfMonth { get; set; }
	}
}