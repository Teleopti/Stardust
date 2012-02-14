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

        #region Properties - Instance Member

        #endregion

        #region Methods - Instance Member

        ISite CreateInstance(string name);
        ICollection<ISite> FindSiteByDescriptionName(string name);

        #endregion

    }

}
