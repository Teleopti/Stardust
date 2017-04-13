using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    /// <summary>
    /// Provides display adapter for site and team.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-10-07
    /// </remarks>
    public class SiteTeamModel : EntityContainer<ITeam>
    {
        private const string Separator = "/";

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-07
        /// </remarks>
        public string Description
        {
            get { return string.Concat(ContainedEntity.Site.Description, Separator, ContainedEntity.Description.Name); }
        }


        /// <summary>
        /// Gets or sets the team.
        /// </summary>
        /// <value>The team.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/14/2009
        /// </remarks>
        public ITeam Team
        {
            get { return ContainedEntity; }
            set { ContainedEntity = value; }
        }

        /// <summary>
        /// Gets the site.
        /// </summary>
        /// <value>The site.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-07
        /// </remarks>
        public ISite Site
        {
            get { return ContainedEntity.Site; }
        }

    }
}
