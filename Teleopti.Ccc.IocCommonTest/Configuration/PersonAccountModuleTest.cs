using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class PersonAccountModuleTest
	{
		[Test]
		public void ShouldResolveQuickForecaster()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(CommonModule.ForTest(new TrueToggleManager()));

			using (var container = containerBuilder.Build())
			{
				container.Resolve<IPersonLeavingUpdater>()
					.Should().Not.Be.Null();
			}
		}
	}
}