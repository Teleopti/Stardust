using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface ILatestStatisticsIntervalIdLoader
	{
		int? Load(Guid[] skillIdList, DateOnly now, TimeZoneInfo userTimeZone);
	}
}