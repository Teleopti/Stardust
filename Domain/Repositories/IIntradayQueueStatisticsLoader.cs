using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IIntradayQueueStatisticsLoader
	{
		LatestStatisticsTimeAndWorkload LoadActualWorkloadInSeconds(IList<Guid> skillIdList, TimeZoneInfo timeZone, DateOnly today);
	}
}