using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData
{
	public interface IHistoricalData
	{
		TaskOwnerPeriod Fetch(IWorkload workload, DateOnlyPeriod period);
	}
}