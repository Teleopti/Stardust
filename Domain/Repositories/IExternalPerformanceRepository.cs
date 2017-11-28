using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IExternalPerformanceRepository
	{
		IEnumerable<ExternalPerformance> FindAllExternalPerformances();
		ExternalPerformance FindExternalPerformanceByExternalId(int externalId);
	}
}