using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
    public class TeleoptiSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        private readonly BusinessUnitFromToken _businessUnitFromToken = new BusinessUnitFromToken();
        private readonly ApplicationUserFromToken _applicationUserFromToken = new ApplicationUserFromToken();
        private readonly ApplicationDataSourceFromToken _applicationDataSourceFromToken = new ApplicationDataSourceFromToken();
        private readonly LicenseFromToken _licenseFromToken = new LicenseFromToken();

        public IBusinessUnit BusinessUnit => _businessUnitFromToken.BusinessUnit;

	    public IDataSource DataSource => _applicationDataSourceFromToken.DataSource;

	    protected override bool CanValidateTokenCore(SecurityToken token)
        {
            // Check that the incoming token is a username token type that  
            // can be validated by this implementation.
            return (token is UserNameSecurityToken);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy>
            ValidateTokenCore(SecurityToken token)
        {
            var userNameToken = token as CustomUserNameSecurityToken;

            if (isIncompleteToken(userNameToken))
            {
                return new ReadOnlyCollection<IAuthorizationPolicy>(new List<IAuthorizationPolicy>());
            }

            _applicationDataSourceFromToken.SetDataSource(userNameToken);
            if (_applicationDataSourceFromToken.DataSourceNotFound())
            {
                throw new FaultException(string.Format(CultureInfo.InvariantCulture, "The datasource {0} cannot be found.", userNameToken.DataSource));
            }

				_licenseFromToken.SetLicense(_applicationDataSourceFromToken.DataSource, userNameToken.DataSource);

            _applicationUserFromToken.SetPersonFromToken(userNameToken,_applicationDataSourceFromToken.DataSource);

            if (userNameToken.HasBusinessUnit)
            {
                _businessUnitFromToken.SetBusinessUnitFromToken(userNameToken,_applicationDataSourceFromToken.DataSource);
            }

            // Create just one Claim instance for the username token - the name of the user.
	        var userNameClaimSet = new DefaultClaimSet(ClaimSet.System,
		        new Claim(ClaimTypes.Name, userNameToken.UserName, Rights.PossessProperty));
            var policies = new List<IAuthorizationPolicy>(1)
            {
	            new TeleoptiPrincipalAuthorizationPolicy(userNameClaimSet, _applicationUserFromToken.PersonContainer)
            };
	        return policies.AsReadOnly();
        }

        private static bool isIncompleteToken(CustomUserNameSecurityToken userNameToken)
        {
            return string.IsNullOrEmpty(userNameToken?.DataSource) || string.IsNullOrEmpty(userNameToken.UserName) || string.IsNullOrEmpty(userNameToken.Password);
        }
    }
}