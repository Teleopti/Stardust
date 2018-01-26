using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IExternalPerformanceDataRepository : IRepository<IExternalPerformanceData>
	{
		ICollection<IExternalPerformanceData> FindByPeriod(DateTimePeriod period);
		ICollection<IExternalPerformanceData> Find(DateTime date, List<Guid> personIds, int performanceId);
		ICollection<Guid> FindPersonsCouldGetBadge(DateTime date, List<Guid> personIds, int performanceId, double badgeThreshold);
	}
}
