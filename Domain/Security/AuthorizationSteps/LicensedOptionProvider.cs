using System.Collections.Generic;
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
    public class LicensedOptionProvider : IAuthorizationEntityProvider<LicenseOption>
    {

        private IList<IApplicationFunction> _inputList;


        #region IAuthorizationEntityProvider<LicenseOption> Members

        /// <summary>
        /// Gets the result entity list.
        /// </summary>
        /// <value>The result entity list.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public IList<LicenseOption> ResultEntityList
        {
            get 
            {
                IList<LicenseOption> resultList = new List<LicenseOption>();

                LicenseSchema currentSchema = LicenseSchema.ActiveLicenseSchema;
                IList<LicenseOption> licenseOptions = currentSchema.LicenseOptions;
                
                if (licenseOptions != null)
                {
                    foreach (LicenseOption licenseOption in licenseOptions)
                    {
                        if (currentSchema.EnabledLicenseSchema == licenseOption.LicenseSchemaCode && licenseOption.Enabled)
                        {
                            licenseOption.EnableApplicationFunctions(_inputList);
                            resultList.Add(licenseOption);
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
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public IList<IAuthorizationEntity> InputEntityList
        {
            set { _inputList = AuthorizationEntityExtender.ConvertToSpecificList<IApplicationFunction>(value); }
            get { return AuthorizationEntityExtender.ConvertToBaseList(_inputList); }
        }

        #endregion



    }
}
