using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IExternalPerformanceRepository
	{
		IEnumerable<IExternalPerformance> FindAllExternalPerformances();
		IExternalPerformance FindExternalPerformanceByExternalId(int externalId);
	}
}