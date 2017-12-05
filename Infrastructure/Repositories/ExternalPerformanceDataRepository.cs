using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalPerformanceDataRepository : Repository<IExternalPerformanceData>, IExternalPerformanceDataRepository
	{
#pragma warning disable 618
		public ExternalPerformanceDataRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning disable 618
		{
		}

		public ExternalPerformanceDataRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
	}
}
