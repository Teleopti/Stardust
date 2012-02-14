using System.Collections.Generic;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// Specification to filter Assignments by agent(s)
    /// </summary>
    public class AssignmentBelongsToAgentSpecification : Specification<IPersonAssignment>
    {
        private readonly List<IPerson> _personCollection = new List<IPerson>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentBelongsToAgentSpecification"/> class.
        /// Used when searching for one agent.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public AssignmentBelongsToAgentSpecification(IPerson agent)
        {
            _personCollection.Add(agent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentBelongsToAgentSpecification"/> class.
        /// Used when searching for a list of agent.
        /// </summary>
        /// <param name="agentList">The agent list.</param>
        public AssignmentBelongsToAgentSpecification(IEnumerable<IPerson> agentList)
        {
            if (agentList != null)
                _personCollection.AddRange(agentList);
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// 	<c>true</c> if [is satisfied by] [the specified obj]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsSatisfiedBy(IPersonAssignment obj)
        {
            foreach (IPerson agent in _personCollection)
            {
                if (obj.Person.Equals(agent))
                    return true;
            }
            return false;
        }
    }
}