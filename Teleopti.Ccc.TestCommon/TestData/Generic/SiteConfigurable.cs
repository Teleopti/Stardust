using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Generic
{
	public class SiteConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public string BusinessUnit { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var site = SiteFactory.CreateSimpleSite(Name);
			var siteRepository = new SiteRepository(uow);
			siteRepository.Add(site);
			var businessUnit = new BusinessUnitRepository(uow).LoadAll().Single(b => b.Name == BusinessUnit);
			businessUnit.AddSite(site);
		}
	}
}