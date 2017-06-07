using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;


namespace Teleopti.Ccc.WinCodeTest.Autofac
{
	[TestFixture]
	public class BackToLegalShiftModuleTest
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void Setup()
		{
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new CommonModule(configuration));
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