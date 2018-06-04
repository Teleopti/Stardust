using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IForecastDayOverrideRepository : IRepository<IForecastDayOverride>
	{
		ICollection<IForecastDayOverride> FindRange(DateOnlyPeriod period, IWorkload workload, IScenario scenario);
	}
}