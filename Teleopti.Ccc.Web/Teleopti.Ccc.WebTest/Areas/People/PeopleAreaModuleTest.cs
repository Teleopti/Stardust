using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Ccc.Web.Areas.People.Core.IoC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[TestFixture]
	public class PeopleAreaModuleTest
	{

		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void SetUp()
		{
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new WebAppModule(configuration));
			_containerBuilder.RegisterModule(new PeopleAreaModule());
		}


		[Test]
		public void ShouldResolveImportAgentController()
		{
			using (var ioc = _containerBuilder.Build())
			{
				using (var scope = ioc.BeginLifetimeScope("WebPeople"))
				{
					var controller = scope.Resolve<ImportAgentController>();
					controller.Should().Not.Be.Null();
				}
			}
		}
	}
}
