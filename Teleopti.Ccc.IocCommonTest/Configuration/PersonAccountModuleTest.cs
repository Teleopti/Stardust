using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class PersonAccountModuleTest
	{
		[TestCase(true, Description =
			"Should resolve IPersonLeavingUpdater with toggle WFM_Clear_Data_After_Leaving_Date_47768 enabled")]
		[TestCase(false, Description =
			"Should resolve IPersonLeavingUpdater with toggle WFM_Clear_Data_After_Leaving_Date_47768 disabled")]
		public void ShouldResolvePersonLeavingUpdater(bool enableToggle47768)
		{
			var toggleManager = new FakeToggleManager();
			if (enableToggle47768)
			{
				toggleManager.Enable(Toggles.WFM_Clear_Data_After_Leaving_Date_47768);
			}
			else
			{
				toggleManager.Disable(Toggles.WFM_Clear_Data_After_Leaving_Date_47768);
			}

			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(CommonModule.ForTest(toggleManager));

			using (var container = containerBuilder.Build())
			{
				container.Resolve<IPersonLeavingUpdater>()
					.Should().Not.Be.Null();
			}
		}
	}
}