using Autofac;
using NUnit.Framework;
using SharpTestsEx;
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
		public void ShouldResolveHangfireEventProcessor()
		{
			using (var container = buildContainer())
			{
				container.Resolve<HangfireEventProcessor>().Should().Not.Be.Null();
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