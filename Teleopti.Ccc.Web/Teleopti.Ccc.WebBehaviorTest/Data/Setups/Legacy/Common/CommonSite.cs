using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common
{
	public class CommonSite : IDataSetup
	{
		public ISite Site;

		public void Apply(IUnitOfWork uow)
		{
			var businessUnit = GlobalDataMaker.Data().Data<CommonBusinessUnit>().BusinessUnit;

			Site = SiteFactory.CreateSimpleSite(RandomName.Make("Common site"));
			var siteRepository = new SiteRepository(uow);
			siteRepository.Add(Site);
			businessUnit.AddSite(Site);
		}
	}
}