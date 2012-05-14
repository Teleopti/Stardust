using Teleopti.Ccc.Domain.Specification;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    /// <summary>
    /// Specification to filter Person on Site.
    /// </summary>
    public class PersonBelongsToSiteSpecification : Specification<IPerson>
    {
        private readonly IList<ISite> _siteCollection;
        private readonly DateOnlyPeriod _dateTimePeriod;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBelongsToSiteSpecification"/> class.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="siteCollection">The site collection.</param>
        public PersonBelongsToSiteSpecification(DateOnlyPeriod dateTimePeriod, IList<ISite> siteCollection)
        {
            _siteCollection = siteCollection;
            _dateTimePeriod = dateTimePeriod;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBelongsToSiteSpecification"/> class.
        /// </summary>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="site">The site.</param>
        public PersonBelongsToSiteSpecification(DateOnlyPeriod dateTimePeriod, ISite site) : this (dateTimePeriod,new []{site})
        {
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// 	<c>true</c> if [is satisfied by] [the specified obj]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsSatisfiedBy(IPerson obj)
        {
            foreach (ISite site in _siteCollection)
            {
                if(new PersonBelongsToTeamSpecification(_dateTimePeriod, site.TeamCollection).IsSatisfiedBy(obj))
                    return true;
            }
            return false;
        }
    }
}