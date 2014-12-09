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

		private ILifetimeScope buildContainer()
		{
			return buildContainer(CommonModule.ToggleManagerForIoc());
		}

		private ILifetimeScope buildContainer(IToggleManager toggleManager)
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(), toggleManager)));
			builder.RegisterModule(new HangfireModule());
			return builder.Build();
		}

	}
}