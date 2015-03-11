using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Rta
{
	public class ResolvedPerson
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}

	public interface IDatabaseReader
	{
		ConcurrentDictionary<string, int> Datasources();
		ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns();
		IList<ScheduleLayer> GetCurrentSchedule(Guid personId);
	}

}