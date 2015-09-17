using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IDatabaseLoader
	{
		ConcurrentDictionary<string, int> Datasources();
		ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns();
		IDictionary<Guid, PersonOrganizationData> PersonOrganizationData();
		IEnumerable<ScheduleLayer> GetCurrentSchedule(Guid personId);
	}
}