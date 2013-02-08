using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Messages.General
{
	/// <summary>
	/// Message with details to perform an quick forecast on a workload.
	/// </summary>
	public class QuickForecastWorkloadMessage : RaptorDomainMessage
	{
		/// <summary>
		/// Same as the JobId
		/// </summary>
		public override Guid Identity
		{
			get { return JobId; }
		}

		/// <summary>
		/// The Id of the workload
		/// </summary>
		public Guid WorkloadId { get; set; }

		/// <summary>
		/// A id identifying this job
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
		/// If the standard templates should be updated
		/// </summary>
		public bool UpdateStandardTemplates { get; set; }
	}
}