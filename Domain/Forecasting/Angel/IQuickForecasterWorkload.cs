using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecasterWorkload
	{
		double Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams);
		ForecastingAccuracy[] Measure(IWorkload workload, DateOnlyPeriod historicalPeriod);
	}
}