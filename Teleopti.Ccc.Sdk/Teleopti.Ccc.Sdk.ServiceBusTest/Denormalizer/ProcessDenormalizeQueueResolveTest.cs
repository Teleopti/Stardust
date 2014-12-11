using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class ProcessDenormalizeQueueResolveTest
	{
		private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private IServiceBus _serviceBus;

		[SetUp]
		public void Setup()
		{
			var mocks = new MockRepository();
			_unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			_serviceBus = mocks.DynamicMock<IServiceBus>();
		}

		[Test]
		public void ShouldResolveProcessDenormalizeQueueConsumer()
		{
			UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;

			var builder = new ContainerBuilder();
			builder.RegisterInstance(_serviceBus).As<IServiceBus>();
			builder.RegisterType<ProcessDenormalizeQueueConsumer>().As<ConsumerOf<ProcessDenormalizeQueue>>();

			builder.RegisterModule(CommonModule.ForTest());
			builder.RegisterModule<ServiceBusCommonModule>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<ExportForecastContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<ProcessDenormalizeQueue>>().Should().Not.Be.Null();
			}
		}
	}
}
