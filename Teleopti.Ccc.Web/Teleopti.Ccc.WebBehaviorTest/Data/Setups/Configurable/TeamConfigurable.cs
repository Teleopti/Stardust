using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class TeamConfigurable : TestCommon.TestData.Setups.Configurable.TeamConfigurable
	{
		public TeamConfigurable()
		{
			Site = GlobalDataMaker.Data().Data<CommonSite>().Site.Description.Name;
		}
	}
}