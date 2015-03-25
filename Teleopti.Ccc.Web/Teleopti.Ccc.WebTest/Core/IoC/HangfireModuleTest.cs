using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.MultipleConfig;
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

		private ILifetimeScope buildContainer()
		{
			return buildContainer(CommonModule.ToggleManagerForIoc(new IocArgs(new AppConfigReader())));
		}

		private ILifetimeScope buildContainer(IToggleManager toggleManager)
		{
			var builder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new AppConfigReader()), toggleManager);
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new HangfireModule(configuration));
			return builder.Build();
		}

	}
}