using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ResolvedPerson
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}

	public interface IDatabaseReader
	{
		ConcurrentDictionary<string, int> Datasources(string tenant);
		ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns(string tenant);
		IList<ScheduleLayer> GetCurrentSchedule(Guid personId, string tenant);
	}

}