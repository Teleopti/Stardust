using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for ApplicationRole - SystemRole mapper root.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 2008-03-17
    /// </remarks>
    public class SystemRoleApplicationRoleMapperRepository : Repository<SystemRoleApplicationRoleMapper>, ISystemRoleApplicationRoleMapperRepository
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemRoleApplicationRoleMapperRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        public SystemRoleApplicationRoleMapperRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// Finds all application role mappers by the system name (that is the name of the foreign system, "ActiveDirectory" for active directory).
        /// </summary>
        /// <remarks>
        /// From the list of role mappers, the Application Roles can be extracted. 
        /// </remarks>
        public IList<SystemRoleApplicationRoleMapper> FindAllBySystemName(string systemName)
        {
            IList<SystemRoleApplicationRoleMapper> retList = Session.CreateCriteria(typeof(SystemRoleApplicationRoleMapper))
                .Add(Expression.Eq("SystemName", systemName))
                .List<SystemRoleApplicationRoleMapper>();
            return retList;
        }
    }
}
