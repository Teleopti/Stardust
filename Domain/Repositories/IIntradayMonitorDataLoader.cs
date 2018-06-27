using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IIntradayMonitorDataLoader
	{
		IList<IncomingIntervalModel> Load(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today);
	}
}