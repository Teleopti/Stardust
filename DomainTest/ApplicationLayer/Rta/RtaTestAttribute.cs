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
		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			builder.UseTestDouble(new FakeMessageSender()).For<IMessageSender>();
			builder.UseTestDouble(new FakeCurrentDatasource()).For<ICurrentDataSource>();
			registerFakePublisher(builder, configuration, new FakeEventPublisher());
			registerFakeDatabase(builder, configuration, new FakeRtaDatabase());

			builder.UseTestDouble(new FakeReadModelUnitOfWorkAspect()).For<IReadModelUnitOfWorkAspect>();
			builder.UseTestDouble(new FakeAllBusinessUnitsUnitOfWorkAspect()).For<IAllBusinessUnitsUnitOfWorkAspect>();
			builder.UseTestDouble(new FakeDistributedLockAcquirer()).For<IDistributedLockAcquirer>();

			builder.UseTestDouble(new FakeTeamOutOfAdherenceReadModelPersister()).For<ITeamOutOfAdherenceReadModelPersister>();
			builder.UseTestDouble(new FakeSiteOutOfAdherenceReadModelPersister()).For<ISiteOutOfAdherenceReadModelPersister>();
			builder.UseTestDouble(new FakeAdherenceDetailsReadModelPersister()).For<IAdherenceDetailsReadModelPersister>();
			builder.UseTestDouble(new FakeAdherencePercentageReadModelPersister()).For<IAdherencePercentageReadModelPersister>();

			builder.AddService(this);
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
		}

		private static void registerFakePublisher(ISystem builder, IIocConfiguration configuration, FakeEventPublisher publisher)
		{
			builder.UseTestDouble(publisher).For<IEventPublisher>();
		}

	}
}