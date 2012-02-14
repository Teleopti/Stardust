using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class ExportMultisiteSkillsToSkillResolveTest
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
		public void ShouldResolveNewAbsenceRequestConsumer()
		{
			UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;

			var builder = new ContainerBuilder();
			builder.RegisterInstance(_serviceBus).As<IServiceBus>();
			builder.RegisterType<ExportMultisiteSkillsToSkillConsumer>().As<ConsumerOf<ExportMultisiteSkillsToSkill>>();

			builder.RegisterModule<RepositoryContainerInstaller>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<ExportForecastContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<ExportMultisiteSkillsToSkill>>().Should().Not.Be.Null();
			}
		}
	}
}
