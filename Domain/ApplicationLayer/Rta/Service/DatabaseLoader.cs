using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class DatabaseLoader : IDatabaseLoader
	{
		private readonly IDatabaseReader _databaseReader;

		public DatabaseLoader(IDatabaseReader databaseReader)
		{
			_databaseReader = databaseReader;
		}

		public IEnumerable<ScheduleLayer> GetCurrentSchedule(Guid personId)
		{
			return _databaseReader.GetCurrentSchedule(personId);
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return _databaseReader.Datasources();
		}

		public IDictionary<Guid, PersonOrganizationData> PersonOrganizationData()
		{
			return _databaseReader.PersonOrganizationData().ToDictionary(data => data.PersonId);
		}

		public ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns()
		{
			return _databaseReader.ExternalLogOns();
		}

	}
}