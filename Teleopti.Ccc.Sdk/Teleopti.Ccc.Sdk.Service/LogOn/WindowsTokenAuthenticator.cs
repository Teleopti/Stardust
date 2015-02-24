using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
    public class WindowsTokenAuthenticator : SecurityTokenAuthenticator
    {
        private readonly LicenseFromToken _licenseFromToken = new LicenseFromToken();
        private readonly WindowsUserFromToken _windowsUserFromToken = new WindowsUserFromToken();
        private readonly BusinessUnitFromToken _businessUnitFromToken = new BusinessUnitFromToken();
        private readonly WindowsDataSourceFromToken _windowsDataSourceFromToken = new WindowsDataSourceFromToken();

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            // Check that the incoming token is a username token type that  
            // can be validated by this implementation.
            return (token is WindowsSecurityToken);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy>
            ValidateTokenCore(SecurityToken token)
        {
            var windowsSecurityToken = token as CustomWindowsSecurityToken;
            
            if (isTokenEmpty(windowsSecurityToken))
            {
                throw new SecurityTokenValidationException("Invalid windows security token.");
            }

            _windowsDataSourceFromToken.SetDataSource(windowsSecurityToken);

            if (_windowsDataSourceFromToken.DataSourceNotFound())
            {
                throw new FaultException(string.Format(CultureInfo.InvariantCulture, "The datasource {0} cannot be found.", windowsSecurityToken.DataSource));
            }

				_licenseFromToken.SetLicense(_windowsDataSourceFromToken.DataSourceContainer, windowsSecurityToken.DataSource);

            _windowsUserFromToken.SetPersonFromToken(windowsSecurityToken,_windowsDataSourceFromToken.DataSourceContainer);

            if (windowsSecurityToken.HasBusinessUnit)
            {
                _businessUnitFromToken.SetBusinessUnitFromToken(windowsSecurityToken, _windowsDataSourceFromToken.DataSourceContainer);
            }

            // Create just one Claim instance for the username token - the name of the user.
	        var userNameClaimSet = new DefaultClaimSet(ClaimSet.Windows,
		        new Claim(ClaimTypes.Name, windowsSecurityToken.WindowsIdentity.Name, Rights.PossessProperty));
            var policies = new List<IAuthorizationPolicy>(1)
            {
	            new TeleoptiPrincipalAuthorizationPolicy(userNameClaimSet, _windowsUserFromToken.PersonContainer)
            };
	        return policies.AsReadOnly();
        }

        public IDataSource DataSource {get { return _windowsDataSourceFromToken.DataSourceContainer.DataSource; }}

        public IBusinessUnit BusinessUnit
        {
            get { return _businessUnitFromToken.BusinessUnit; }
        }

        private static bool isTokenEmpty(CustomWindowsSecurityToken windowsSecurityToken)
        {
            return windowsSecurityToken==null;
        }
    }
}