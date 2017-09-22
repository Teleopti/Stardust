using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class TeamConfigurable : TestCommon.TestData.Setups.Configurable.TeamConfigurable
	{
		public override void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			if (Site == null)
			{
				var site = new SiteConfigurable { BusinessUnit = DefaultBusinessUnit.BusinessUnit.Name };
				DataMaker.Data().Apply(site);
				Site = site.Name;
			}

			base.Apply(currentUnitOfWork);
		}
	}
}