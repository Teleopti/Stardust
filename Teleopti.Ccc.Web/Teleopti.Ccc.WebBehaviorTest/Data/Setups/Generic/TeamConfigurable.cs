using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class TeamConfigurable : TestCommon.TestData.Generic.TeamConfigurable
	{
		public TeamConfigurable()
		{
			Site = GlobalDataMaker.Data().Data<CommonSite>().Site.Description.Name;
		}
	}
}