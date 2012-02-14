using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// SystemRole - ApplicationRole mapper entity provider.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/27/2007
    /// </remarks>
    public class MappedActiveDirectoryRolesProvider : IAuthorizationEntityProvider<IApplicationRole>
    {
        private IList<IAuthorizationEntity> _inputList;
        private readonly ISystemRoleApplicationRoleMapperRepository _rep;
        private readonly string _systemName;


        /// <summary>
        /// Initializes a new instance of the <see cref="MappedActiveDirectoryRolesProvider"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="systemName">Name of the system.</param>
        public MappedActiveDirectoryRolesProvider(ISystemRoleApplicationRoleMapperRepository repository, string systemName)
        {
            _rep = repository;
            _systemName = systemName;
        }

        #region IAuthorizationEntityProvider<SystemRoleApplicationRoleMapper> Members


        /// <summary>
        /// Gets the result entity list.
        /// </summary>
        /// <value>The result entity list.</value>
        public IList<IApplicationRole> ResultEntityList
        {
            get
            {
                IList<IApplicationRole> resultList = new List<IApplicationRole>();

                IList<SystemRoleApplicationRoleMapper> repositoryList = _rep.FindAllBySystemName(_systemName);

                if (repositoryList != null)
                {
                    foreach (SystemRoleApplicationRoleMapper mapper in repositoryList)
                    {
                        if (AuthorizationEntityExtender.IsAnyAuthorizationKeyEquals(_inputList, mapper.AuthorizationKey))
                        {
                            if (!resultList.Contains(mapper.ApplicationRole))
                            {
                                resultList.Add(mapper.ApplicationRole);
                            }
                        }
                    }
                }

                return resultList;
            }
        }

        /// <summary>
        /// Sets the parent entity list.
        /// </summary>
        /// <value>The parent entity list.</value>
        /// <remarks>
        /// Used for setting the parent result when needed for the operation.
        /// </remarks>
        public IList<IAuthorizationEntity> InputEntityList
        {
            set { _inputList = value; }
        }

        #endregion
    }
}
