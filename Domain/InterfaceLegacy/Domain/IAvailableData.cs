using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Contains permission information about the data that is available for an
    /// application role. 
    /// </summary>
    public interface IAvailableData : IAggregateRoot
    {
        /// <summary>
        /// Gets or sets the application role.
        /// </summary>
        /// <value>The application role.</value>
        IApplicationRole ApplicationRole { get; set; }

        /// <summary>
        /// Gets the available business units.
        /// </summary>
        /// <value>The available business units.</value>
        ReadOnlyCollection<IBusinessUnit> AvailableBusinessUnits { get; }

        /// <summary>
        /// Gets the available sites.
        /// </summary>
        /// <value>The available sites.</value>
        ReadOnlyCollection<ISite> AvailableSites { get; }

        /// <summary>
        /// Gets the available teams.
        /// </summary>
        /// <value>The available teams.</value>
        ReadOnlyCollection<ITeam> AvailableTeams { get; }

        /// <summary>
        /// Gets or sets the available data range.
        /// </summary>
        /// <value>The available data range.</value>
        AvailableDataRangeOption AvailableDataRange { get; set; }
		
        /// <summary>
        /// Adds an available business unit.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        void AddAvailableBusinessUnit(IBusinessUnit businessUnit);

        /// <summary>
        /// Adds an available site.
        /// </summary>
        /// <param name="site">The site.</param>
        void AddAvailableSite(ISite site);

        /// <summary>
        /// Adds an available team.
        /// </summary>
        /// <param name="team">The team.</param>
        void AddAvailableTeam(ITeam team);

        /// <summary>
        /// Deletes the availableteam.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/2/2008
        /// </remarks>
        void DeleteAvailableTeam(ITeam team);

        /// <summary>
        /// Deletes the available site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/2/2008
        /// </remarks>
        void DeleteAvailableSite(ISite site);

        /// <summary>
        /// Deletes the available business unit.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/2/2008
        /// </remarks>
        void DeleteAvailableBusinessUnit(IBusinessUnit businessUnit);
    }
}
