using System;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class EvaluateInput
	{
		public Guid WorkloadId { get; set; }
	}


	public class QueueStatisticsInput
	{
		public Guid WorkloadId { get; set; }
		public ForecastMethodType ForecastMethodType { get; set; }
	}
}