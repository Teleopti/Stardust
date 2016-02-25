using Teleopti.Ccc.TestCommon.TestData.Setups.Default;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class SiteConfigurable : TestCommon.TestData.Setups.Configurable.SiteConfigurable
	{
		public SiteConfigurable()
		{
			BusinessUnit = DefaultBusinessUnit.BusinessUnitFromFakeState.Name;
		}
	}
}