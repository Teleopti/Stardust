using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class SiteConfigurable : TestCommon.TestData.Generic.SiteConfigurable
	{
		public SiteConfigurable()
		{
			BusinessUnit = GlobalDataMaker.Data().Data<CommonBusinessUnit>().BusinessUnit;
		}
	}
}