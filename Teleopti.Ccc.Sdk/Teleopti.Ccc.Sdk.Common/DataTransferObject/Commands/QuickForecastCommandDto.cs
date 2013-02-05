using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command runs a quick forecast on the service bus
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class QuickForecastCommandDto : CommandDto
    {
		/// <summary>
		/// The Id of the scenario to forecast
		/// </summary>
		[DataMember]
		public Guid ScenarioId { get; set; }

		/// <summary>
		/// The period to base the forecast on
		/// </summary>
		[DataMember]
		public DateOnlyPeriodDto StatisticPeriod { get; set; }

		/// <summary>
		/// The period to forecast
		/// </summary>
		[DataMember]
		public DateOnlyPeriodDto TargetPeriod { get; set; }

		/// <summary>
		/// If the standard templates should be updated
		/// </summary>
		[DataMember]
		public bool UpdateStandardTemplates { get; set; }

		/// <summary>
		/// The workloads to forecast
		/// </summary>
		[DataMember]
		public ICollection<Guid> WorkloadIds { get; set; }
    }
}
