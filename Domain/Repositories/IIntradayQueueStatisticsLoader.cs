using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IIntradayQueueStatisticsLoader
	{
		int? LoadActualWorkloadInSeconds(IList<Guid> skillIdList, TimeZoneInfo timeZone, DateOnly today);
	}
}