using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Class for UnitCollection
    /// </summary>
    public interface ISite : IAggregateRoot, 
                                IFilterOnBusinessUnit
    {
		/// <summary>
        /// Gets or sets the descrition of the site.
        /// </summary>
        /// <value>The site.</value>
        Description Description { get; }

	    void SetDescription(Description value);

		void SetBusinessUnit(IBusinessUnit value);


		/// <summary>
		/// Gets the teams.
		/// Read only wrapper around the actual list.
		/// </summary>
		/// <value>The teams.</value>
		ReadOnlyCollection<ITeam> TeamCollection { get; }

		/// <summary>
		/// Gets the teams.
		/// Sorted on description, ascending.
		/// </summary>
    	ReadOnlyCollection<ITeam> SortedTeamCollection { get; }

		/// <summary>
		/// Gets or sets the max seats.
		/// </summary>
		/// <value>The max seats.</value>
    	int? MaxSeats { get; set; }

    	/// <summary>
        /// Adds a Team.
        /// </summary>
        /// <param name="team">The team.</param>
        void AddTeam(ITeam team);

        /// <summary>
        /// Removes the team.
        /// </summary>
        /// <param name="team">The team.</param>
		void RemoveTeam(ITeam team);
		IEnumerable<ISiteOpenHour> OpenHourCollection { get; }
		void ClearOpenHourCollection();
		bool AddOpenHour(ISiteOpenHour siteOpenHour);

		string UpdatedTimeInUserPerspective { get; }

	}
}
