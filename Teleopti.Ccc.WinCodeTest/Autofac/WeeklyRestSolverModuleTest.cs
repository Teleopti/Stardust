using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Autofac
{
	[TestFixture]
	public class WeeklyRestSolverModuleTest
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void Setup()
		{
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new CommonModule(configuration));
			_containerBuilder.RegisterModule(new SchedulingCommonModule(configuration));
			_containerBuilder.RegisterModule(new RuleSetModule(configuration,true));
		}

		[Test]
		public void ShouldResolve()
		{
			_containerBuilder.RegisterModule<WeeklyRestSolverModule>();
			using (var container = _containerBuilder.Build())
			{
				container.Resolve<IWeeklyRestSolverService>()
								 .Should().Not.Be.Null();
			}
		}
	}
}