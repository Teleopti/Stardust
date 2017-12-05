using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IExternalPerformanceRepository : IAggregateRoot, IRepository<IExternalPerformance>
	{
		IEnumerable<IExternalPerformance> FindAllExternalPerformances();
		IExternalPerformance FindExternalPerformanceByExternalId(int externalId);
		int GetExernalPerformanceCount();
	}
}