using System.Collections.Generic;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    /// <summary>
    /// Specification to filter Person on Site.
    /// </summary>
    public class TeamBelongsToSiteSpecification : Specification<ITeam>
    {
        private readonly IList<ISite> _siteCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamBelongsToSiteSpecification"/> class.
        /// </summary>
        /// <param name="siteCollection">The site collection.</param>
        public TeamBelongsToSiteSpecification(IList<ISite> siteCollection)
        {
            _siteCollection = siteCollection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamBelongsToSiteSpecification"/> class.
        /// </summary>
        /// <param name="site">The site.</param>
        public TeamBelongsToSiteSpecification(ISite site)
        {
            _siteCollection = new List<ISite> { site };
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// 	<c>true</c> if [is satisfied by] [the specified obj]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsSatisfiedBy(ITeam obj)
        {
            foreach (ISite site in _siteCollection)
            {
                if(site.TeamCollection.Contains(obj))
                    return true;
            }
            return false;
        }
    }
}