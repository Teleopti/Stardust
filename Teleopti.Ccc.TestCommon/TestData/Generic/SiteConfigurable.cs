using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Generic
{
    public class SiteConfigurable : IDataSetup
    {
        public string Name { get; set; }

        public IBusinessUnit BusinessUnit;

        public void Apply(IUnitOfWork uow)
        {
            var site = SiteFactory.CreateSimpleSite(Name);
            var siteRepository = new SiteRepository(uow);
            siteRepository.Add(site);
            BusinessUnit.AddSite(site);
        }
    }
}