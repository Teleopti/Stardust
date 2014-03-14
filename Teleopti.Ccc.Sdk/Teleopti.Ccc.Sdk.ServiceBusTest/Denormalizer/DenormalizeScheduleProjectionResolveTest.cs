using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class DenormalizeScheduleProjectionResolveTest
	{
		private IServiceBus _serviceBus;

		[SetUp]
		public void Setup()
		{
			var mocks = new MockRepository();
			_serviceBus = mocks.DynamicMock<IServiceBus>();
		}

		[Test]
		public void ShouldResolveProcessDenormalizeQueueConsumer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance(_serviceBus).As<IServiceBus>();
			builder.RegisterType<ScheduleProjectionReadOnlyUpdater>().As<IHandleEvent<ScheduledResourcesChangedEvent>>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<ExportForecastContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();
			builder.RegisterModule<EventHandlersModule>();
			builder.RegisterType<NoJsonSerializer>().As<IJsonSerializer>();
			builder.RegisterModule<AuthenticationModule>();
			builder.RegisterModule<UnitOfWorkModule>();
			builder.RegisterModule<LocalServiceBusPublisherModule>();
			builder.RegisterModule<DateAndTimeModule>();

			using (var container = builder.Build())
			{
				container.Resolve<IHandleEvent<ScheduledResourcesChangedEvent>>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveScheduleDayReadModelHandlerConsumer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance(_serviceBus).As<IServiceBus>();
			builder.RegisterType<ScheduleDayReadModelHandler>().As<IHandleEvent<ProjectionChangedEvent>>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<ExportForecastContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();
			builder.RegisterModule<EventHandlersModule>();
			builder.RegisterType<NoJsonSerializer>().As<IJsonSerializer>();
			builder.RegisterType<DoNotNotifySmsLink>().As<IDoNotifySmsLink>();
			builder.RegisterType<LocalServiceBusPublisher>().As<IPublishEventsFromEventHandlers>().SingleInstance();

			using (var container = builder.Build())
			{
				container.Resolve<IHandleEvent<ProjectionChangedEvent>>().Should().Not.Be.Null();
			}
		}

        [Test]
		public void ShouldResolvePersonScheduleDayReadModelHandlerConsumer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance(_serviceBus).As<IServiceBus>();
			builder.RegisterType<PersonScheduleDayReadModelUpdater>().As<IHandleEvent<ProjectionChangedEvent>>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<ExportForecastContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();
			builder.RegisterModule<EventHandlersModule>();
			builder.RegisterType<NoJsonSerializer>().As<IJsonSerializer>();
			builder.RegisterType<DoNotNotifySmsLink>().As<IDoNotifySmsLink>();
			builder.RegisterType<LocalServiceBusPublisher>().As<IPublishEventsFromEventHandlers>().SingleInstance();

			using (var container = builder.Build())
			{
				container.Resolve<IHandleEvent<ProjectionChangedEvent>>().Should().Not.Be.Null();
			}
		}
	}

	public class NoJsonSerializer : IJsonSerializer
	{
		public string SerializeObject(object value)
		{
			throw new System.NotImplementedException();
		}
	}
}