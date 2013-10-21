using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class SiteConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var businessUnit = GlobalDataMaker.Data().Data<CommonBusinessUnit>().BusinessUnit;

			var site = SiteFactory.CreateSimpleSite(Name);
			var siteRepository = new SiteRepository(uow);
			siteRepository.Add(site);
			businessUnit.AddSite(site);
		}
	}
}