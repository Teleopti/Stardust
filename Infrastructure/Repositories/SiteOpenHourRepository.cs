using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SiteOpenHourRepository : Repository<ISiteOpenHour>, ISiteOpenHourRepository
	{
#pragma warning disable 618
		public SiteOpenHourRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
        {
		}

		public SiteOpenHourRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {

		}
	}
}
