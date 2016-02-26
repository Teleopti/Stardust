using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IIntradayMonitorDataLoader
	{
		MonitorDataViewModel Load(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today);
	}
}