using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// Licenced application function entity provider
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 05/11/2008
    /// </remarks>
    public class LicensedApplicationFunctionsProvider : IAuthorizationEntityProvider<IApplicationFunction>
    {
        private IList<LicenseOption> _inputList;

        #region IAuthorizationEntityProvider<ApplicationFunction> Members

        /// <summary>
        /// Gets the result entity list.
        /// </summary>
        /// <value>The result entity list.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 05/11/2008
        /// </remarks>
        public IList<IApplicationFunction> ResultEntityList
        {
            get 
            {
                IList<IApplicationFunction> resultList = new List<IApplicationFunction>();
                if (_inputList != null)
                {
                    foreach (LicenseOption licenseOption in _inputList)
                    {
                        AuthorizationEntityExtender.UnionTwoLists(resultList, licenseOption.EnabledApplicationFunctions);
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
        /// Created date: 05/11/2008
        /// </remarks>
        public IList<IAuthorizationEntity> InputEntityList
        {
            set { _inputList = AuthorizationEntityExtender.ConvertToSpecificList<LicenseOption>(value); }
            get { return AuthorizationEntityExtender.ConvertToBaseList(_inputList); }
        }

        #endregion
    }
}
