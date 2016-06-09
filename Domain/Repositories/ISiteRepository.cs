using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface ISiteRepository : IRepository<ISite>
    {
        ICollection<ISite> FindSiteByDescriptionName(string name);
	    IEnumerable<ISite> FindSitesContain(string searchString, int maxHits);
    }
}
