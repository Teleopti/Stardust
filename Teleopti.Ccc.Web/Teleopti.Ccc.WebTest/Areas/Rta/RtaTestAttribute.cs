using Autofac;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class RtaTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterModule(new RtaModule(configuration));

			builder.RegisterInstance(new FakeMessageSender()).As<IMessageSender>().AsSelf();
			builder.RegisterInstance(new FakeCurrentDatasource()).As<ICurrentDataSource>().AsSelf();
			builder.RegisterInstance(new FakeEventPublisher()).As<IEventPublisher>().AsSelf();
			builder.RegisterInstance(new FakeRtaDatabase())
				.As<IDatabaseReader>()
				.As<IDatabaseWriter>()
				.As<IPersonOrganizationReader>()
				.As<IReadActualAgentStates>()
				.AsSelf()
				;
			builder.RegisterInstance(new FakeMbCacheFactory()).As<IMbCacheFactory>().AsSelf();
		}
	}
}