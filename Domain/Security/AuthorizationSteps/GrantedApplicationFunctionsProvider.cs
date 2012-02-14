using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// Granted application function entity provider
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/27/2007
    /// </remarks>
    public class GrantedApplicationFunctionsProvider : IAuthorizationEntityProvider<IApplicationFunction>
    {
        private IList<IApplicationRole> _inputList;

        #region IAuthorizationEntityProvider<ApplicationFunction> Members

        /// <summary>
        /// Gets the result entity list.
        /// </summary>
        /// <value>The result entity list.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public IList<IApplicationFunction> ResultEntityList
        {
            get 
            {
                IList<IApplicationFunction> resultList = new List<IApplicationFunction>();
                if (_inputList != null)
                {
                    List<IApplicationRole> list = new List<IApplicationRole>(_inputList.OfType<IApplicationRole>());
                    foreach (ApplicationRole role in list)
                    {
                        AuthorizationEntityExtender.UnionTwoLists(resultList, role.ApplicationFunctionCollection);
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
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public IList<IAuthorizationEntity> InputEntityList
        {
            set { _inputList = AuthorizationEntityExtender.ConvertToSpecificList<IApplicationRole>(value); }
            get { return AuthorizationEntityExtender.ConvertToBaseList(_inputList); }
        }

        #endregion
    }
}
