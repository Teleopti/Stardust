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
		ConcurrentDictionary<string, int> Datasources();
		IList<ScheduleLayer> GetCurrentSchedule(Guid personId);

		ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns();
		IEnumerable<PersonOrganizationData> PersonOrganizationData();

		IEnumerable<PersonOrganizationData> LoadPersonData(int dataSourceId, string externalLogOn);
		IEnumerable<PersonOrganizationData> LoadAllPersonsData();
	}

}