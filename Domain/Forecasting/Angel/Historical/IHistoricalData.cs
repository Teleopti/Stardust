using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Historical
{
	public interface IHistoricalData
	{
		TaskOwnerPeriod Fetch(IWorkload workload, DateOnlyPeriod period);
	}
}