using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SiteOpenHourRepository : Repository<ISiteOpenHour>, ISiteOpenHourRepository
	{
		public SiteOpenHourRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {

		}
	}
}
