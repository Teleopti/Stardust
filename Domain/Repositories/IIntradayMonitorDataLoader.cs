using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IIntradayMonitorDataLoader
	{
		IList<IncomingIntervalModel> Load(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today);
	}
}