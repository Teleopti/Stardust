using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IExternalPerformanceRepository : IRepository<IExternalPerformance>
	{
		IEnumerable<IExternalPerformance> FindAllExternalPerformances();
		IExternalPerformance FindExternalPerformanceByExternalId(int externalId);
		int GetExernalPerformanceCount();
		void UpdateExternalPerformanceName(Guid id, string name);
		void UpdateExternalPerformanceName(int qualityId, int dataType, string name);
	}
}