using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeDatabaseReader : IDatabaseReader
	{
		private readonly INow _now;
		private readonly FakeDataSources _dataSources;
		private readonly FakePersonRepository _persons;

		public FakeDatabaseReader(
			INow now, 
			FakeDataSources dataSources, 
			FakePersonRepository persons)
		{
			_now = now;
			_dataSources = dataSources;
			_persons = persons;
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return new ConcurrentDictionary<string, int>(
				_dataSources
					.Datasources
					.GroupBy(x => x.Key, (key, g) => g.First()
					));
		}


		public IEnumerable<PersonOrganizationData> LoadPersonOrganizationData(int dataSourceId, string externalLogOn)
		{
			return userDatas()
				.Where(x =>
					x.Data.UserCode == externalLogOn &&
					x.DataSourceId == dataSourceId)
				.Select(m => m.Data)
				.ToArray();
		}

		public IEnumerable<PersonOrganizationData> LoadPersonOrganizationDatas(int dataSourceId, IEnumerable<string> externalLogOns)
		{
			return userDatas()
				.Where(x =>
					externalLogOns.Contains(x.Data.UserCode) &&
					x.DataSourceId == dataSourceId)
				.Select(m => m.Data)
				.ToArray();
		}

		public IEnumerable<PersonOrganizationData> LoadAllPersonOrganizationData()
		{
			return userDatas()
				.Select(x => x.Data);
		}

		private class userData
		{
			public int DataSourceId;
			public PersonOrganizationData Data;
		}

		private IEnumerable<userData> userDatas()
		{
			return _persons.LoadAll()
				.Select(x => x.Period(new DateOnly(_now.UtcDateTime())))
				.Where(x => x != null)
				.SelectMany(x =>
					x.ExternalLogOnCollection.Select(e => new
					{
						externalLogon = e,
						period = x
					}))
				.Select(x =>
					new userData
					{
						DataSourceId = x.externalLogon.DataSourceId,
						Data = new PersonOrganizationData
						{
							UserCode = x.externalLogon.AcdLogOnOriginalId,
							PersonId = x.period.Parent.Id.Value,
							TeamId = x.period.Team.Id.Value,
							SiteId = x.period.Team.Site.Id.Value,
							BusinessUnitId = x.period.Team.Site.BusinessUnit.Id.Value,
						}
					})
				.ToArray();
		}
	}
}