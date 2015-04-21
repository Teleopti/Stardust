using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class RtaTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterInstance(new FakeMessageSender()).As<IMessageSender>().AsSelf();
			builder.RegisterInstance(new FakeCurrentDatasource()).As<ICurrentDataSource>().AsSelf();
			registerFakePublisher(builder, configuration, new FakeEventPublisher());
			registerFakeDatabase(builder, configuration, new FakeRtaDatabase());

			builder.RegisterInstance(new FakeReadModelUnitOfWorkAspect()).As<IReadModelUnitOfWorkAspect>();
			builder.RegisterInstance(new FakeAllBusinessUnitsUnitOfWorkAspect()).As<IAllBusinessUnitsUnitOfWorkAspect>().AsSelf();
			builder.RegisterInstance(new FakeDistributedLockAcquirer()).As<IDistributedLockAcquirer>();

			builder.RegisterInstance(new FakeTeamOutOfAdherenceReadModelPersister()).As<ITeamOutOfAdherenceReadModelPersister>().AsSelf();
			builder.RegisterInstance(new FakeSiteOutOfAdherenceReadModelPersister()).As<ISiteOutOfAdherenceReadModelPersister>().AsSelf();
			builder.RegisterInstance(new FakeAdherenceDetailsReadModelPersister()).As<IAdherenceDetailsReadModelPersister>().AsSelf();
			builder.RegisterInstance(new FakeAdherencePercentageReadModelPersister()).As<IAdherencePercentageReadModelPersister>().AsSelf();

			builder.RegisterInstance(this);
		}

		public void SimulateRestartWith(MutableNow now, FakeRtaDatabase database, FakeEventPublisher publisher)
		{
			Reset((b, c) =>
			{
				registerFakeDatabase(b, c, database);
				registerFakePublisher(b, c, publisher);
				b.RegisterInstance(now).As<INow>().AsSelf();
			});
		}

		private static void registerFakeDatabase(ContainerBuilder builder, IIocConfiguration configuration, FakeRtaDatabase database)
		{
			builder.RegisterInstance(database)
				.As<IDatabaseReader>()
				.As<IDatabaseWriter>()
				.As<IPersonOrganizationReader>()
				.AsSelf()
				;
			builder.RegisterInstance(database.AgentStateReadModelReader).As<IAgentStateReadModelReader>();
			builder.RegisterInstance(database.RtaStateGroupRepository).As<IRtaStateGroupRepository>();
			builder.RegisterInstance(database.StateGroupActivityAlarmRepository).As<IStateGroupActivityAlarmRepository>();
		}

		private static void registerFakePublisher(ContainerBuilder builder, IIocConfiguration configuration, FakeEventPublisher publisher)
		{
			builder.RegisterInstance(publisher).As<IEventPublisher>().AsSelf();
		}

	}
}