using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeDataSourcesFactoryWithEvents : IDataSourcesFactory
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IFakeStorage _storage;
		private readonly IEnumerable<ITransactionHook> _hooks;
		private readonly INow _now;

		public FakeDataSourcesFactoryWithEvents(IFakeStorage storage, IEventPopulatingPublisher eventPublisher, IEnumerable<ITransactionHook> hooks, INow now)
		{
			_eventPublisher = eventPublisher;
			_storage = storage;
			_hooks = hooks;
			_now = now;
		}

		public IDataSource Create(IDictionary<string, string> applicationNhibConfiguration, string statisticConnectionString)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage, _eventPublisher, _hooks, _now),
				new FakeAnalyticsUnitOfWorkFactory { ConnectionString = statisticConnectionString },
				new FakeReadModelUnitOfWorkFactory()
			);
		}

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage, _eventPublisher, _hooks, _now) { ConnectionString = applicationConnectionString, Name = tenantName },
				new FakeAnalyticsUnitOfWorkFactory { ConnectionString = statisticConnectionString },
				new FakeReadModelUnitOfWorkFactory()
			);
		}

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString, IDictionary<string, string> applicationNhibConfiguration)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage, _eventPublisher, _hooks, _now) { ConnectionString = applicationConnectionString, Name = tenantName },
				new FakeAnalyticsUnitOfWorkFactory { ConnectionString = statisticConnectionString },
				new FakeReadModelUnitOfWorkFactory()
			);
		}
	}

	public class FakeDataSourcesFactory : IDataSourcesFactory
	{
		private readonly IFakeStorage _storage;

		public FakeDataSourcesFactory(IFakeStorage storage)
		{
			_storage = storage;
		}

		public IDataSource Create(IDictionary<string, string> applicationNhibConfiguration, string statisticConnectionString)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage, null, null, null),
				new FakeAnalyticsUnitOfWorkFactory { ConnectionString = statisticConnectionString },
				null
				);
		}

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage, null, null, null) { ConnectionString = applicationConnectionString, Name = tenantName },
				new FakeAnalyticsUnitOfWorkFactory { ConnectionString = statisticConnectionString },
				null
				);
		}

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString, IDictionary<string, string> applicationNhibConfiguration)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage, null, null, null) { ConnectionString = applicationConnectionString, Name = tenantName },
				new FakeAnalyticsUnitOfWorkFactory { ConnectionString = statisticConnectionString },
				null
				);
		}
	}
}