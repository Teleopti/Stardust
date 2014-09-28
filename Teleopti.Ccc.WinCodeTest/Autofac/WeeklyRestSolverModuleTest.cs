using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.WinCode.Autofac;

namespace Teleopti.Ccc.WinCodeTest.Autofac
{
	[TestFixture]
	public class WeeklyRestSolverModuleTest
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void Setup()
		{
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule<CommonModule>();
			_containerBuilder.RegisterModule<SchedulingCommonModule>();

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