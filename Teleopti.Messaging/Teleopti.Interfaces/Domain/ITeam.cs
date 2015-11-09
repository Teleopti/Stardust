namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Class for team
    /// </summary>
    public interface ITeam : IAggregateRoot
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        Description Description { get; set; }

        /// <summary>
        /// Gets or sets the site.
        /// </summary>
        /// <value>The site.</value>
        ISite Site { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is choosable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is choosable; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-09
        /// </remarks>
        bool IsChoosable { get; }

        /// <summary>
        /// Gets the site and team as a string.
        /// </summary>
        /// <value>The site and team.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-04-20
        /// </remarks>
        string SiteAndTeam { get; }

        /// <summary>
        /// Gets the business unit explicitly.
        /// </summary>
        /// <value>The business unit.</value>
        IBusinessUnit BusinessUnitExplicit { get; }

        ///<summary>
        /// The scorecard to use for this team.
        ///</summary>
        IScorecard Scorecard { get; set; }
    }
}
