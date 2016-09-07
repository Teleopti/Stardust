using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IDatabaseReader
	{
		ConcurrentDictionary<string, int> Datasources();

		IEnumerable<ScheduledActivity> GetCurrentSchedule(DateTime utcNow, Guid personId);
		IEnumerable<ScheduledActivity> GetCurrentSchedules(DateTime utcNow, IEnumerable<Guid> personIds);

		IEnumerable<PersonOrganizationData> LoadPersonOrganizationData(int dataSourceId, string externalLogOn);
		IEnumerable<PersonOrganizationData> LoadPersonOrganizationDatas(int dataSourceId, IEnumerable<string> externalLogOns);
		IEnumerable<PersonOrganizationData> LoadAllPersonOrganizationData();
	}
	
	public class PersonOrganizationData
	{
		public string UserCode { get; set; }
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
	}
}