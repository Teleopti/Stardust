using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class RtaTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeMessageSender()).For<IMessageSender>();
			system.UseTestDouble(new FakeCurrentDatasource()).For<ICurrentDataSource>();
			registerFakePublisher(system, configuration, new FakeEventPublisher());
			registerFakeDatabase(system, configuration, new FakeRtaDatabase());

			system.UseTestDouble(new FakeReadModelUnitOfWorkAspect()).For<IReadModelUnitOfWorkAspect>();
			system.UseTestDouble(new FakeAllBusinessUnitsUnitOfWorkAspect()).For<IAllBusinessUnitsUnitOfWorkAspect>();
			system.UseTestDouble(new FakeDistributedLockAcquirer()).For<IDistributedLockAcquirer>();

			system.UseTestDouble(new FakeTeamOutOfAdherenceReadModelPersister()).For<ITeamOutOfAdherenceReadModelPersister>();
			system.UseTestDouble(new FakeSiteOutOfAdherenceReadModelPersister()).For<ISiteOutOfAdherenceReadModelPersister>();
			system.UseTestDouble(new FakeAdherenceDetailsReadModelPersister()).For<IAdherenceDetailsReadModelPersister>();
			system.UseTestDouble(new FakeAdherencePercentageReadModelPersister()).For<IAdherencePercentageReadModelPersister>();

			system.AddService(this);
		}

		public void SimulateRestartWith(MutableNow now, FakeRtaDatabase database, FakeEventPublisher publisher)
		{
			Reset((b, c) =>
			{
				registerFakeDatabase(b, c, database);
				registerFakePublisher(b, c, publisher);
				b.UseTestDouble(now).For<INow>();
			});
		}

		private static void registerFakeDatabase(ISystem builder, IIocConfiguration configuration, FakeRtaDatabase database)
		{
			builder.UseTestDouble(database)
				.For<IDatabaseReader, IDatabaseWriter, IPersonOrganizationReader>()
				;
			builder.UseTestDouble(database.AgentStateReadModelReader).For<IAgentStateReadModelReader>();
			builder.UseTestDouble(database.RtaStateGroupRepository).For<IRtaStateGroupRepository>();
			builder.UseTestDouble(database.StateGroupActivityAlarmRepository).For<IStateGroupActivityAlarmRepository>();
			builder.UseTestDouble(database.Tenants).For<IFindTenantByRtaKey, ICountTenants, ILoadAllTenants>();
		}

		private static void registerFakePublisher(ISystem builder, IIocConfiguration configuration, FakeEventPublisher publisher)
		{
			builder.UseTestDouble(publisher).For<IEventPublisher>();
		}

	}
}