using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestData
	{
		public static IBusinessUnit BusinessUnit { get { return CommonBusinessUnit.BusinessUnitFromFakeState; } }

		public static string CommonPassword = "1";

		public static IApplicationRole AgentRole;
	}
}
