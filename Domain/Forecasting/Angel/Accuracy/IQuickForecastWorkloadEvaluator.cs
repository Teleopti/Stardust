using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IQuickForecastWorkloadEvaluator
	{
		WorkloadAccuracy Measure(IWorkload workload, DateOnlyPeriod historicalPeriod);
	}
}