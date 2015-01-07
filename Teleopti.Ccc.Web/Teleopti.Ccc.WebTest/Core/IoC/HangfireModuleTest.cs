using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Core.Hangfire;

namespace Teleopti.Ccc.WebTest.Core.IoC
{
	[TestFixture]
	public class HangfireModuleTest
	{
		[Test]
		public void ShouldResolveHangfireEventServer()
		{
			using (var container = buildContainer())
			{
				container.Resolve<HangfireEventServer>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveHangfireEventClient()
		{
			using (var container = buildContainer())
			{
				container.Resolve<IHangfireEventClient>().Should().Be.OfType<HangfireEventClient>();
			}
		}

		[Test]
		public void ShouldResolveHangfireMsmqStorageConfigurationIfToggleEnabled()
		{
			using (var container = buildContainer(Toggles.RTA_HangfireEventProcessinUsingMsmq_31237, true))
			{
				container.Resolve<IHangfireServerStorageConfiguration>().Should().Be.OfType<MsmqStorageConfiguration>();
			}
		}

		[Test]
		public void ShouldResolveHangfireSqlStorageConfigurationIfToggleDisabled()
		{
			using (var container = buildContainer(Toggles.RTA_HangfireEventProcessinUsingMsmq_31237, false))
			{
				container.Resolve<IHangfireServerStorageConfiguration>().Should().Be.OfType<SqlStorageConfiguration>();
			}
		}

		private ILifetimeScope buildContainer()
		{
			return buildContainer(CommonModule.ToggleManagerForIoc());
		}

		private ILifetimeScope buildContainer(IToggleManager toggleManager)
		{
			var builder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(), toggleManager);
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new HangfireModule(configuration));
			return builder.Build();
		}

		private ILifetimeScope buildContainer(Toggles toggle, bool value)
		{
			var toggleManager = MockRepository.GenerateStub<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(toggle)).Return(value);
			return buildContainer(toggleManager);
		}

	}
}