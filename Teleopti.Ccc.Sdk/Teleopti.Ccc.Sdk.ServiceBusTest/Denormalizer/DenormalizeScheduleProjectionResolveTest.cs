using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class DenormalizeScheduleProjectionResolveTest
	{
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IServiceBus _serviceBus;

		[SetUp]
		public void Setup()
		{
			var mocks = new MockRepository();
			_unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			_serviceBus = mocks.DynamicMock<IServiceBus>();
		}

		[Test]
		public void ShouldResolveProcessDenormalizeQueueConsumer()
		{
			UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;

			var builder = new ContainerBuilder();
			builder.RegisterInstance(_serviceBus).As<IServiceBus>();
			builder.RegisterType<ScheduleProjectionHandler>().As<IHandleEvent<ProjectionChangedEvent>>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<ExportForecastContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<IHandleEvent<ProjectionChangedEvent>>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveScheduleDayReadModelHandlerConsumer()
		{
			UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;

			var builder = new ContainerBuilder();
			builder.RegisterInstance(_serviceBus).As<IServiceBus>();
			builder.RegisterType<ScheduleDayReadModelHandler>().As<IHandleEvent<ProjectionChangedEvent>>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<ExportForecastContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<IHandleEvent<ProjectionChangedEvent>>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolvePersonScheduleDayReadModelHandlerConsumer()
		{
			UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;

			var builder = new ContainerBuilder();
			builder.RegisterInstance(_serviceBus).As<IServiceBus>();
			builder.RegisterType<PersonScheduleDayReadModelHandler>().As<IHandleEvent<ProjectionChangedEvent>>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<ExportForecastContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<IHandleEvent<ProjectionChangedEvent>>().Should().Not.Be.Null();
			}
		}
	}
}