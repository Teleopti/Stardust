using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastingAccuracy
	{
		public Guid WorkloadId { get; set; }
		public IList<MethodAccuracy> Accuracies { get; set; }
	}
}