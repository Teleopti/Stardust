using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// Application role entity provider
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/27/2007
    /// </remarks>
    public class RaptorApplicationRolesProvider : IAuthorizationEntityProvider<IApplicationRole>
    {
        private IPerson _person;
        private IList<IAuthorizationEntity> _inputList;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private readonly IPersonRepository _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="RaptorApplicationRolesProvider"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="repository">The repository.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public RaptorApplicationRolesProvider(IPerson person, IPersonRepository repository)
        {
            _person = person;
            _rep = repository;
        }

        #region IAuthorizationEntityProvider<IApplicationRole> Members

        /// <summary>
        /// Gets the result entity list.
        /// </summary>
        /// <value>The result entity list.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public IList<IApplicationRole> ResultEntityList
        {
            get 
            {
                //_person = _rep.LoadPermissionData(_person);
                IList<IApplicationRole> resultList = new List<IApplicationRole>();
                foreach (IApplicationRole role in _person.PermissionInformation.ApplicationRoleCollection)
                {
                    resultList.Add(role);
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
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public IList<IAuthorizationEntity> InputEntityList
        {
            set { _inputList = value; }
            protected get { return _inputList; }
        }

        #endregion
    }
}
