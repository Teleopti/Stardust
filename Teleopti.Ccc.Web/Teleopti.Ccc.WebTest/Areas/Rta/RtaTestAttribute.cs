using Autofac;
using MbCache.Core;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Interfaces.Domain;
using IMessageSender = Teleopti.Interfaces.MessageBroker.Client.IMessageSender;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class RtaTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterModule(new RtaModule(configuration));

			builder.RegisterInstance(new FakeMessageSender()).As<IMessageSender>().AsSelf();
			builder.RegisterInstance(new FakeCurrentDatasource()).As<ICurrentDataSource>().AsSelf();
			builder.RegisterInstance(new FakeMbCacheFactory()).As<IMbCacheFactory>().AsSelf();
			registerFakePublisher(builder, new FakeEventPublisher());
			registerFakeDatabase(builder, new FakeRtaDatabase());

			builder.RegisterInstance(new FakeReadModelUnitOfWorkAspect()).As<IReadModelUnitOfWorkAspect>();
			builder.RegisterInstance(new FakeTeamOutOfAdherenceReadModelPersister()).As<ITeamOutOfAdherenceReadModelPersister>().AsSelf();
			builder.RegisterInstance(new FakeSiteAdherencePersister()).As<ISiteAdherencePersister>().AsSelf();
			builder.RegisterInstance(new FakeAdherenceDetailsReadModelPersister()).As<IAdherenceDetailsReadModelPersister>().AsSelf();

			builder.RegisterInstance(this);
		}

		public void SimulateRestartWith(MutableNow now, FakeRtaDatabase database, FakeEventPublisher publisher)
		{
			Reset(b =>
			{
				registerFakeDatabase(b, database);
				registerFakePublisher(b, publisher);
				b.RegisterInstance(now).As<INow>().AsSelf();
			});
		}

		private static void registerFakeDatabase(ContainerBuilder builder, FakeRtaDatabase database)
		{
			builder.RegisterInstance(database)
				.As<IDatabaseReader>()
				.As<IDatabaseWriter>()
				.As<IPersonOrganizationReader>()
				.As<IReadActualAgentStates>()
				.AsSelf()
				;
		}

		private static void registerFakePublisher(ContainerBuilder builder, FakeEventPublisher publisher)
		{
			builder.RegisterInstance(publisher).As<IEventPublisher>().AsSelf();
		}

	}
}