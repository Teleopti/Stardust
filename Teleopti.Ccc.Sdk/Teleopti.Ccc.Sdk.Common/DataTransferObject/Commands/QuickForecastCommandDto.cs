using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command runs a quick forecast on the service bus
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class QuickForecastCommandDto : CommandDto
    {
		public QuickForecastCommandDto ()
		{
			WorkloadIds = new Collection<Guid>();
			SmoothingStyle = 5;
			StatisticPeriod = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto {DateTime = DateTime.Today.AddYears(-2)},
					EndDate = new DateOnlyDto {DateTime = DateTime.Today.AddDays(-1)}
				};
			var start = DateTime.Today.AddMonths(1);
			start = new DateTime(start.Year,start.Month,1);
			var end = start.AddMonths(3).AddDays(-1);
			TargetPeriod = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = start },
				EndDate = new DateOnlyDto { DateTime = end }
			};

			TemplatePeriod = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = DateTime.Today.AddMonths(-3).AddDays(-1) },
				EndDate = new DateOnlyDto { DateTime = DateTime.Today.AddDays(-1) }
			};
		}

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
		/// The smoothing style of the templates
		/// </summary>
		[DataMember]
		public int SmoothingStyle { get; set; }

		/// <summary>
		/// The period to get the temlplates from
		/// </summary>
		[DataMember]
		public DateOnlyPeriodDto TemplatePeriod { get; set; }

		/// <summary>
		/// The workloads to forecast
		/// </summary>
		[DataMember]
		public ICollection<Guid> WorkloadIds { get; set; }
    }
}
