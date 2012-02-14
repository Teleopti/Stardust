using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    ///  Interface for SystemRoleApplicationRoleMapperRepository.
    /// </summary>
    public interface ISystemRoleApplicationRoleMapperRepository : IRepository<SystemRoleApplicationRoleMapper>
    {

        /// <summary>
        /// Finds all the SystemRoleApplicationRoleMapper entities by the system name.
        /// </summary>
        /// <param name="systemName">Name of the system.</param>
        /// <returns></returns>
        IList<SystemRoleApplicationRoleMapper> FindAllBySystemName(string systemName);
    }
}