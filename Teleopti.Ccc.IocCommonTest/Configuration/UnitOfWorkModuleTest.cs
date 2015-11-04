using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class UnitOfWorkModuleTest
	{
		private ContainerBuilder builder;
		private IContainer container;

		[SetUp]
		public void Setup()
		{
			builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
			container = builder.Build();
		}

		[Test]
		public void ShouldResolveLicenseActivatorProvider()
		{
			container.Resolve<ILicenseActivatorProvider>().Should().Be.OfType<LicenseActivatorProvider>();
		}

		[Test]
		public void ShouldResolveCurrentPersistCallbacks()
		{
			container.Resolve<ICurrentPersistCallbacks>().Should().Be.OfType<CurrentPersistCallbacks>();
		}

		[Test]
		public void ShouldResolveMessageSendersScope()
		{
			container.Resolve<IMessageSendersScope>().Should().Be.OfType<CurrentPersistCallbacks>();
		}
	}
}