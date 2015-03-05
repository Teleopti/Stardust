using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IIndexVolumes
	{
		IEnumerable<IVolumeYear> Create(ITaskOwnerPeriod historicalData);
	}
}