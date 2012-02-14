using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    public interface ITeamCollection
    {
        /// <summary>
        /// Gets all permitted teams.
        /// </summary>
        /// <value>All permitted teams.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-06-09
        /// </remarks>
        IEnumerable<ITeam> AllPermittedTeams { get; }

        /// <summary>
        /// Gets all permitted sites.
        /// </summary>
        /// <value>All permitted sites.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-06-09
        /// </remarks>
        IEnumerable<ISite> AllPermittedSites { get; }
    }
}