using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IDatabaseReader
	{
		ConcurrentDictionary<string, int> Datasources();
		IList<ScheduledActivity> GetCurrentSchedule(Guid personId);

		IEnumerable<PersonOrganizationData> LoadPersonOrganizationData(int dataSourceId, string externalLogOn);
		IEnumerable<PersonOrganizationData> LoadAllPersonOrganizationData();
	}
	
	public class PersonOrganizationData
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
	}
}