using System;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Models
{
	public class QueueStatisticsInput
	{
		public Guid WorkloadId { get; set; }
		public ForecastMethodType MethodId { get; set; }
	}
}