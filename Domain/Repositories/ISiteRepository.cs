using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface ISiteRepository : IRepository<ISite>
    {
        ICollection<ISite> FindSiteByDescriptionName(string name);
	    IEnumerable<ISite> FindSitesContain(string searchString, int maxHits);
	    IEnumerable<ISite> LoadAllOrderByName();
	    IEnumerable<ISite> LoadAllWithFetchingOpenHours();
    }
}
