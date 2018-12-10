using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IExternalPerformanceDataRepository : IRepository<IExternalPerformanceData>
	{
		ICollection<IExternalPerformanceData> FindByPeriod(DateOnlyPeriod period);
		ICollection<IExternalPerformanceData> FindPersonsCouldGetBadgeOverThreshold(DateOnly date, List<Guid> personIds, int performanceId, double badgeThreshold, Guid businessId);
	}
}
