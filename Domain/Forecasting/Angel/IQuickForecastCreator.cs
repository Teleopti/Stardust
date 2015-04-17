using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecastCreator
	{
		void CreateForecastForWorkloads(DateOnlyPeriod futurePeriod, ForecastWorkloadInput[] workloads);
		void CreateForecastForAll(DateOnlyPeriod futurePeriod);
	}
}