using System;
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
    public class TeleoptiSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        private readonly BusinessUnitFromToken _businessUnitFromToken = new BusinessUnitFromToken();
        private readonly ApplicationUserFromToken _applicationUserFromToken = new ApplicationUserFromToken();
        private readonly ApplicationDataSourceFromToken _applicationDataSourceFromToken = new ApplicationDataSourceFromToken();
        private readonly LicenseFromToken _licenseFromToken = new LicenseFromToken();

        public IBusinessUnit BusinessUnit
        {
            get { return _businessUnitFromToken.BusinessUnit; }
        }

        public IDataSource DataSource
        {
            get { return _applicationDataSourceFromToken.DataSourceContainer.DataSource; }
        }

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

            if (IsIncompleteToken(userNameToken))
            {
                return new ReadOnlyCollection<IAuthorizationPolicy>(new List<IAuthorizationPolicy>());
            }

            _applicationDataSourceFromToken.SetDataSource(userNameToken);
            if (_applicationDataSourceFromToken.DataSourceNotFound())
            {
                throw new FaultException(string.Format(CultureInfo.InvariantCulture, "The datasource {0} cannot be found.", userNameToken.DataSource));
            }

            _licenseFromToken.SetLicense(_applicationDataSourceFromToken.DataSourceContainer);

            _applicationUserFromToken.SetPersonFromToken(userNameToken,_applicationDataSourceFromToken.DataSourceContainer);

            if (userNameToken.HasBusinessUnit)
            {
                _businessUnitFromToken.SetBusinessUnitFromToken(userNameToken,_applicationDataSourceFromToken.DataSourceContainer);
            }

            // Create just one Claim instance for the username token - the name of the user.
            DefaultClaimSet userNameClaimSet = new DefaultClaimSet(
                ClaimSet.System,
                new Claim(ClaimTypes.Name, userNameToken.UserName, Rights.PossessProperty));
            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
            policies.Add(new TeleoptiPrincipalAuthorizationPolicy(userNameClaimSet, _applicationUserFromToken.PersonContainer));
            return policies.AsReadOnly();
        }

        private static bool IsIncompleteToken(CustomUserNameSecurityToken userNameToken)
        {
            return userNameToken == null ||
                   string.IsNullOrEmpty(userNameToken.DataSource) ||
                   string.IsNullOrEmpty(userNameToken.UserName) ||
                   string.IsNullOrEmpty(userNameToken.Password);
        }
    }
}