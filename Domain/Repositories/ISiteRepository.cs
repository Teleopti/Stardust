#region Imports

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.Domain.Repositories
{

    /// <summary>
    /// Defines the functionality of a .
    /// </summary>
    public interface ISiteRepository : IRepository<ISite>
    {
        ICollection<ISite> FindSiteByDescriptionName(string name);
	    IEnumerable<ISite> FindSitesStartWith(string searchString);
    }

}
