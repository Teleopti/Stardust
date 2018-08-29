using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IForecastWorkloadEvaluator
	{
		WorkloadAccuracy Evaluate(IWorkload workload, IOutlierRemover outlierRemover, IForecastAccuracyCalculator forecastAccuracyCalculator);
	}
}