using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class ForecasterModuleTest
	{
		[Test]
		public void ShouldResolveQuickForecaster()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(CommonModule.ForTest());

			using (var container = containerBuilder.Build())
			{
				container.Resolve<IQuickForecaster>()
					.Should().Not.Be.Null();
			}
		}
	}
}