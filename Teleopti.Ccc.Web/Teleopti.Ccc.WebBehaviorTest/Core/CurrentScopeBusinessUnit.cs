using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{
	public static class CurrentScopeBusinessUnit
	{
		public static void Reset()
		{
			var businessUnitScope = IntegrationIoCTest.Container.Resolve<IBusinessUnitScope>();
			businessUnitScope.OnThisThreadUse(null);
		}

		public static void Use(BusinessUnitConfigurable businessUnitConfigurable)
		{
			var businessUnitScope = IntegrationIoCTest.Container.Resolve<IBusinessUnitScope>();
			businessUnitScope.OnThisThreadUse(businessUnitConfigurable.BusinessUnit);
		}
	}
}