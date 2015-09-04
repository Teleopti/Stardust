using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[TestFixture]
	public class ResourcePlannerModuleTest
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void Setup()
		{
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new CommonModule(configuration));
			_containerBuilder.RegisterModule(new SchedulingCommonModule());
			_containerBuilder.RegisterModule(new ResourcePlannerModule());
		}

		[Test]
		public void ShouldResolveClassicDaysOffOptimizationCommand()
		{

			using (var container = _containerBuilder.Build())
			{
				container.Resolve<IClassicDaysOffOptimizationCommand>()
								 .Should().Not.Be.Null();
			}
		}
	}
}