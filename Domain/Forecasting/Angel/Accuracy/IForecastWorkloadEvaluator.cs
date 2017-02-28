using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IForecastWorkloadEvaluator
	{
		WorkloadAccuracy Evaluate(IWorkload workload);
	}
}