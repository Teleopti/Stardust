using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class TeamConfigurable : TestCommon.TestData.Setups.Configurable.TeamConfigurable
	{
		public override void Apply(IUnitOfWork uow)
		{
			if (Site == null)
			{
				var site = new SiteConfigurable { BusinessUnit = GlobalDataMaker.Data().Data<CommonBusinessUnit>().BusinessUnit.Name };
				DataMaker.Data().Apply(site);
				Site = site.Name;
			}

			base.Apply(uow);
		}
	}
}