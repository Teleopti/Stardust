using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SiteConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public string BusinessUnit { get; set; }
		public ISite Site { get; private set; }

		public void Apply(IUnitOfWork uow)
		{
			Site = SiteFactory.CreateSimpleSite(Name);
			var siteRepository = new SiteRepository(uow);
			siteRepository.Add(Site);
			var businessUnit = new BusinessUnitRepository(uow).LoadAll().Single(b => b.Name == BusinessUnit);
			businessUnit.AddSite(Site);
		}
	}
}