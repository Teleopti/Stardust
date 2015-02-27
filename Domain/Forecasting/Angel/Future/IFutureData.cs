using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public interface IFutureData
	{
		IEnumerable<ITaskOwner> Fetch(QuickForecasterWorkloadParams quickForecasterWorkloadParams);
	}
}