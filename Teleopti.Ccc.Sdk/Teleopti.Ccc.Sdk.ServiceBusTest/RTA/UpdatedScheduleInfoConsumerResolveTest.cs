using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationRtaQueue;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.TeleoptiRtaService;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
	[TestFixture]
	public class UpdatedScheduleInfoConsumerResolveTest
	{
		[Test]
		public void ShouldResolvePersonScheduleDayReadModelHandlerConsumer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance(MockRepository.GenerateMock<IServiceBus>()).As<IServiceBus>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();
			builder.RegisterInstance(MockRepository.GenerateMock<ITeleoptiRtaService>()).As<ITeleoptiRtaService>();
			builder.RegisterModule<AuthenticationModule>();
			builder.RegisterModule<UnitOfWorkModule>();
			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<LocalServiceBusPublisherModule>();

			builder.RegisterModule<EventHandlersModule>();

			using (var container = builder.Build())
			{
				container.Resolve<IHandleEvent<UpdatedScheduleDay>>().Should().Not.Be.Null();
			}
		}
	}
}