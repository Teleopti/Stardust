using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeDataSourcesFactory : IDataSourcesFactory
	{
		private readonly FakeStorage _storage;

		public FakeDataSourcesFactory(FakeStorage storage)
		{
			_storage = storage;
		}

		public IDataSource Create(IDictionary<string, string> applicationNhibConfiguration, string statisticConnectionString)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage),
				new FakeAnalyticsUnitOfWorkFactory { ConnectionString = statisticConnectionString},
				null
				);
		}

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage) {ConnectionString = applicationConnectionString, Name = tenantName},
				new FakeAnalyticsUnitOfWorkFactory { ConnectionString = statisticConnectionString},
				null
				);
		}

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString, IDictionary<string, string> applicationNhibConfiguration)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage) {ConnectionString = applicationConnectionString, Name = tenantName},
				new FakeAnalyticsUnitOfWorkFactory { ConnectionString = statisticConnectionString},
				null
				);
		}
	}
}