﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IExternalPerformanceDataRepository : IRepository<IExternalPerformanceData>
	{
		ICollection<IExternalPerformanceData> FindByPeriod(DateOnlyPeriod period);
		ICollection<IExternalPerformanceData> Find(DateOnly date, List<Guid> personIds, int performanceId, Guid businessId);
		ICollection<Guid> FindPersonsCouldGetBadgeOverThreshold(DateOnly date, List<Guid> personIds, int performanceId, double badgeThreshold, Guid businessId);
	}
}
