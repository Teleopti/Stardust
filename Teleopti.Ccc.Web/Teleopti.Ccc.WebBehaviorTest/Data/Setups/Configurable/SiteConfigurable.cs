using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class SiteConfigurable : TestCommon.TestData.Setups.Configurable.SiteConfigurable
	{
		public SiteConfigurable()
		{
			BusinessUnit = GlobalDataMaker.Data().Data<DefaultBusinessUnit>().BusinessUnit.Name;
		}
	}
}