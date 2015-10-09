using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Autofac
{
	[TestFixture]
	class IntraIntervalOptimizationServiceModuleTest
	{
		[Test]
		public void Setup()
		{
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new CommonModule(configuration));
			containerBuilder.RegisterModule(new SchedulingCommonModule(configuration));
			containerBuilder.RegisterModule(new RuleSetModule(configuration, true));
			containerBuilder.RegisterModule(new IntraIntervalOptimizationServiceModule(configuration));
			using (var container = containerBuilder.Build())
			{
				container.Resolve<IIntraIntervalOptimizationService>().Should().Not.Be.Null();
			}
		}
	}
}
