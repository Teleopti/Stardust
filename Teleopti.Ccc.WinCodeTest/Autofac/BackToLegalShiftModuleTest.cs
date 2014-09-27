using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.WinCode.Autofac;


namespace Teleopti.Ccc.WinCodeTest.Autofac
{
	[TestFixture]
	public class BackToLegalShiftModuleTest
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
			_containerBuilder.RegisterModule<BackToLegalShiftModule>();
			using (var container = _containerBuilder.Build())
			{
				container.Resolve<IBackToLegalShiftService>()
								 .Should().Not.Be.Null();
			}
		}
	}
}