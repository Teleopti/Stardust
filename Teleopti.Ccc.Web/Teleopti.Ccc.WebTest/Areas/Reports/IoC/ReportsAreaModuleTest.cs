using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.Reports.Controllers;
using Teleopti.Ccc.Web.Areas.Reports.Core;
using Teleopti.Ccc.Web.Areas.Reports.IoC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Reports.IoC
{
	[TestFixture]
	public class ReportsAreaModuleTest
	{
		private ContainerBuilder _containerBuilder;
		private string _requestTag;

		[SetUp]
		public void SetUp()
		{
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new WebAppModule(configuration));
			_containerBuilder.RegisterModule(new ReportsAreaModule());
			_requestTag = "AutofacWebRequest";

		}


		[Test]
		public void ShouldResolveReportsController()
		{
			using (var ioc = _containerBuilder.Build())
			{
				using (var scope = ioc.BeginLifetimeScope(_requestTag))
				{
					var controller = scope.Resolve<ReportsController>();
					controller.Should().Not.Be.Null();
				}
			}
		}

		[Test]
		public void ShouldResolveReportsNavigationProvider()
		{			
			using (var ioc = _containerBuilder.Build())
			{
				using (var scope = ioc.BeginLifetimeScope(_requestTag))
				{
					var handler = scope.Resolve<IReportNavigationProvider>();
					handler.Should().Not.Be.Null();
				}
			}
		}
	}
}