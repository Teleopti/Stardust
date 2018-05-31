using System;
using Teleopti.Ccc.Domain.Forecasting.Models;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface IForecastViewModelFactory
	{
		WorkloadEvaluateViewModel Evaluate(Guid workloadId);
		WorkloadEvaluateMethodsViewModel EvaluateMethods(Guid workloadId);
	}
}