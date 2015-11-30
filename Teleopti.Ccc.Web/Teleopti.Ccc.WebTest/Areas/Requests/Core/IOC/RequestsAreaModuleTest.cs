using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.Requests.Controller;
using Teleopti.Ccc.Web.Areas.Requests.Core.IOC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC
{
	[TestFixture]
	public class RequestsAreaModuleTest
	{
		private ContainerBuilder _containerBuilder;
		private string _requestTag;

		[SetUp]
		public void SetUp()
		{
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new WebAppModule(configuration));
			_containerBuilder.RegisterModule(new RequestsAreaModule());
			_requestTag = "AutofacWebRequest";

		}


		[Test]
		public void ShouldResolveOutboundController()
		{
			using (var ioc = _containerBuilder.Build())
			{
				using (var scope = ioc.BeginLifetimeScope(_requestTag))
				{
					var controller = scope.Resolve<RequestsController>();
					controller.Should().Not.Be.Null();
				}
			}
		}
	}
}