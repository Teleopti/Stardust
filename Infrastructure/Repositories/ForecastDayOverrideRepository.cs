using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ForecastDayOverrideRepository : Repository<IForecastDayOverride>
	{
		public ForecastDayOverrideRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{

		}
	}
}