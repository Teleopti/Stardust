using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IForecastWorkloadEvaluator
	{
		WorkloadAccuracy Evaluate(IWorkload workload);
	}
}