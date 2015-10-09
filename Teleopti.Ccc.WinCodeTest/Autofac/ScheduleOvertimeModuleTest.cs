using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.WinCode.Autofac;

namespace Teleopti.Ccc.WinCodeTest.Autofac
{
	public class ScheduleOvertimeModuleTest
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void Setup()
		{
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new CommonModule(configuration));
			_containerBuilder.RegisterModule(new SchedulingCommonModule(configuration));
		}

		[Test]
		public void ShouldResolve()
		{
			_containerBuilder.RegisterModule<ScheduleOvertimeModule>();
			using (var container = _containerBuilder.Build())
			{
				container.Resolve<IScheduleOvertimeService>().Should().Not.Be.Null();
			}
		}
	}
}
