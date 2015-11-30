using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
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

		private static ILifetimeScope buildContainer()
		{
			return buildContainer(CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader())));
		}

		private static ILifetimeScope buildContainer(IToggleManager toggleManager)
		{
			var builder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), toggleManager);
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new HangfireModule(configuration));
			builder.RegisterType<SetNoLicenseActivator>().As<ISetLicenseActivator>().SingleInstance(); //should probably use infratest attr for these tests instead.
			return builder.Build();
		}

	}
}