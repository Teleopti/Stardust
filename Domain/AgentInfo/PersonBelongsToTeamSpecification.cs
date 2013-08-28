using System.Collections.Generic;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
    /// <summary>
    /// Specification to filter Person on Team.
    /// </summary>
    public class PersonBelongsToTeamSpecification : Specification<IPerson>
    {
        private readonly IList<ITeam> _teamCollection;
        private readonly DateOnlyPeriod _dateOnlyPeriod;
        
        public PersonBelongsToTeamSpecification(DateOnlyPeriod period, ITeam team) : this(period,new []{team})
        {
        }

        public PersonBelongsToTeamSpecification(DateOnlyPeriod period, IEnumerable<ITeam> teams)
		{
			_teamCollection = new List<ITeam>(teams);
			_dateOnlyPeriod = period;
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
            IList<IPersonPeriod> personPeriods = obj.PersonPeriods(_dateOnlyPeriod);
            foreach (IPersonPeriod personPeriod in personPeriods)
            {
                if (_teamCollection.Contains(personPeriod.Team))
                {
                    return true;
                }
            }
            return false;
        }
    }
}