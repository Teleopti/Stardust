using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Insights.Core
{
	[TestFixture]
	public class InsightsAreaRegistrationTest
	{
		[Test]
		public void ShouldResolvePowerBiReportProvider()
		{
			using (var container = buildContainer())
			{
				container.Resolve<IReportProvider>().Should().Be.OfType<ReportProvider>();
			}
		}

		[Test]
		public void ShouldResolvePowerBiClientFactory()
		{
			using (var container = buildContainer())
			{
				container.Resolve<IPowerBiClientFactory>().Should().Be.OfType<PowerBiClientFactory>();
			}
		}

		private static ILifetimeScope buildContainer()
		{
			var iocArg = new IocArgs(new ConfigReader());
			var toggleManager = CommonModule.ToggleManagerForIoc(iocArg);
			return buildContainer(toggleManager);
		}

		private static ILifetimeScope buildContainer(IToggleManager toggleManager)
		{
			var builder = new ContainerBuilder();
			var iocArg = new IocArgs(new ConfigReader());
			var configuration = new IocConfiguration(iocArg, toggleManager);
			builder.RegisterModule(new WebAppModule(configuration));
			builder.RegisterType<NoLicenseServiceInitialization>()
				.As<IInitializeLicenseServiceForTenant>()
				.SingleInstance();
			return builder.Build();
		}
	}
}