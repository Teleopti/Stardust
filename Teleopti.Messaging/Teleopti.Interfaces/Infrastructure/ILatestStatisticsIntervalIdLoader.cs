using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface ILatestStatisticsIntervalIdLoader
	{
		int? Load(Guid[] skillIdList, DateOnly now, TimeZoneInfo userTimeZone);
	}
}