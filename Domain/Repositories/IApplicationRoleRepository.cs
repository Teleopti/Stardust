using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    ///  Interface for ApplicationRoleRepository.
    /// </summary>
    public interface IApplicationRoleRepository : IRepository<IApplicationRole>
    {
        /// <summary>
        /// Reads all application roles ordered by their name.
        /// </summary>
        /// <returns>The ApplicationRole list.</returns>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 08-04-2008
        /// </remarks>
        IList<IApplicationRole> LoadAllApplicationRolesSortedByName();

	    IList<IApplicationRole> LoadAllRolesByDescription(string name);
    }
}