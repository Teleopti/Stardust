using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeDataSourcesFactory : IDataSourcesFactory
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IFakeStorage _storage;
		private readonly IEnumerable<ITransactionHook> _hooks;

		public FakeDataSourcesFactory(IFakeStorage storage, IEventPopulatingPublisher eventPublisher, IEnumerable<ITransactionHook> hooks)
		{
			_eventPublisher = eventPublisher;
			_storage = storage;
			_hooks = hooks;
		}

		public IDataSource Create(IDictionary<string, string> applicationNhibConfiguration, string statisticConnectionString)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage, _eventPublisher, _hooks),
				new FakeAnalyticsUnitOfWorkFactory {ConnectionString = statisticConnectionString},
				new FakeReadModelUnitOfWorkFactory()
			);
		}

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage, _eventPublisher, _hooks) {ConnectionString = applicationConnectionString, Name = tenantName},
				new FakeAnalyticsUnitOfWorkFactory {ConnectionString = statisticConnectionString},
				new FakeReadModelUnitOfWorkFactory()
			);
		}

		public IDataSource Create(string tenantName, string applicationConnectionString, string statisticConnectionString, IDictionary<string, string> applicationNhibConfiguration)
		{
			return new DataSource(
				new FakeUnitOfWorkFactory(_storage, _eventPublisher, _hooks) {ConnectionString = applicationConnectionString, Name = tenantName},
				new FakeAnalyticsUnitOfWorkFactory {ConnectionString = statisticConnectionString},
				new FakeReadModelUnitOfWorkFactory()
			);
		}
	}

	public class FakeDataSourcesFactoryNoEvents : FakeDataSourcesFactory
	{
		public FakeDataSourcesFactoryNoEvents(IFakeStorage storage) : base(storage, null, null)
		{
		}
	}
}