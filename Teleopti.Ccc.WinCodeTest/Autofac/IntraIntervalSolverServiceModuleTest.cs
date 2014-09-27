using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.WinCode.Autofac;


namespace Teleopti.Ccc.WinCodeTest.Autofac
{
	[TestFixture]
	public class IntraIntervalSolverServiceModuleTest
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
			_containerBuilder.RegisterModule<IntraIntervalSolverServiceModule>();
			using (var container = _containerBuilder.Build())
			{
				container.Resolve<IIntraIntervalFinderService>()
								 .Should().Not.Be.Null();
			}
		}
	}
}