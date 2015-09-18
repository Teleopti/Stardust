using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class RtaTestAttribute : IoCTestAttribute
	{
		public RtaTenants Tenants;

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new TenantServerModule(configuration));
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.UseTestDouble<TenantUnitOfWorkFake>().For<ITenantUnitOfWork>();

			system.UseTestDouble<FakeMessageSender>().For<IMessageSender>();
			system.UseTestDouble<FakeApplicationData>().For<IApplicationData, ICurrentApplicationData, IDataSourceForTenant>();
			system.UseTestDouble<FakeCurrentDatasource>().For<ICurrentDataSource>();
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			registerFakeDatabase(system, configuration, null);

			system.UseTestDouble(new FakeReadModelUnitOfWorkAspect()).For<IReadModelUnitOfWorkAspect>();
			system.UseTestDouble(new FakeAllBusinessUnitsUnitOfWorkAspect()).For<IAllBusinessUnitsUnitOfWorkAspect>();
			system.UseTestDouble(new FakeDistributedLockAcquirer()).For<IDistributedLockAcquirer>();

			system.AddService(this);
		}

		public void SimulateRestartWith(MutableNow now, FakeRtaDatabase database)
		{
			Reset((b, c) =>
			{
				registerFakeDatabase(b, c, database);
				b.UseTestDouble(now).For<INow>();
			});
		}

		private static void registerFakeDatabase(ISystem system, IIocConfiguration configuration, FakeRtaDatabase database)
		{
			if (database != null)
			{
				system.UseTestDouble(database).For<IDatabaseReader, IDatabaseWriter>();

				system.UseTestDouble(database.AgentStateReadModelReader).For<IAgentStateReadModelReader>();
				system.UseTestDouble(database.RtaStateGroupRepository).For<IRtaStateGroupRepository>();
				system.UseTestDouble(database.StateGroupActivityAlarmRepository).For<IStateGroupActivityAlarmRepository>();
				system.UseTestDouble(database.Tenants).For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants>();

				system.UseTestDouble(database.TeamOutOfAdherenceReadModelPersister).For<ITeamOutOfAdherenceReadModelPersister>();
				system.UseTestDouble(database.SiteOutOfAdherenceReadModelPersister).For<ISiteOutOfAdherenceReadModelPersister>();
				system.UseTestDouble(database.AdherenceDetailsReadModelPersister).For<IAdherenceDetailsReadModelPersister>();
				system.UseTestDouble(database.AdherencePercentageReadModelPersister).For<IAdherencePercentageReadModelPersister>();
			}
			else
			{
				system.UseTestDouble<FakeRtaDatabase>().For<IDatabaseReader, IDatabaseWriter>();

				system.UseTestDouble<FakeAgentStateReadModelReader>().For<IAgentStateReadModelReader>();
				system.UseTestDouble<FakeRtaStateGroupRepository>().For<IRtaStateGroupRepository>();
				system.UseTestDouble<FakeStateGroupActivityAlarmRepository>().For<IStateGroupActivityAlarmRepository>();
				system.UseTestDouble<FakeTenants>().For<IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants>();

				system.UseTestDouble<FakeTeamOutOfAdherenceReadModelPersister>().For<ITeamOutOfAdherenceReadModelPersister>();
				system.UseTestDouble<FakeSiteOutOfAdherenceReadModelPersister>().For<ISiteOutOfAdherenceReadModelPersister>();
				system.UseTestDouble<FakeAdherenceDetailsReadModelPersister>().For<IAdherenceDetailsReadModelPersister>();
				system.UseTestDouble<FakeAdherencePercentageReadModelPersister>().For<IAdherencePercentageReadModelPersister>();
			}
		}
		
	}
}