using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Repository for skill types
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-24
    /// </remarks>
    public interface ISkillTypeRepository : IRepository<ISkillType>
    {
        /// <summary>
        /// Finds all and include workload and queues.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-12-10
        /// </remarks>
        ICollection<ISkillType> FindAll();
    }
}