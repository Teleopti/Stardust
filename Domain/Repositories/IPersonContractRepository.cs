using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Interface for AgentContractRepository
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-06
    /// </remarks>
    public interface IPersonContractRepository
    {
        /// <summary>
        /// Find agent contracts for the given agents
        /// </summary>
        /// <param name="agents">Agents to search for agent contracts</param>
        /// <returns></returns>
        ICollection<PersonContract> Find(IEnumerable<Person> agents);

        /// <summary>
        /// Find agent contracts for the given agents
        /// </summary>
        /// <param name="agents">Agents to search for agent contracts</param>
        /// <param name="period">Date and time limit</param>
        /// <returns></returns>
        ICollection<PersonContract> Find(IEnumerable<Person> agents, DateTimePeriod period);
    }
}