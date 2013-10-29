using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class SiteConfigurable : TestCommon.TestData.Setups.Configurable.SiteConfigurable
	{
		public SiteConfigurable()
		{
			BusinessUnit = GlobalDataMaker.Data().Data<CommonBusinessUnit>().BusinessUnit.Name;
		}
	}
}