using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class AnotherSite : IDataSetup
	{
		public ISite Site;

		public void Apply(IUnitOfWork uow)
		{
			var businessUnit = GlobalDataMaker.Data().Data<CommonBusinessUnit>().BusinessUnit;

			Site = SiteFactory.CreateSimpleSite("Another Site");
			var siteRepository = new SiteRepository(uow);
			siteRepository.Add(Site);
			businessUnit.AddSite(Site);
		}
	}
}