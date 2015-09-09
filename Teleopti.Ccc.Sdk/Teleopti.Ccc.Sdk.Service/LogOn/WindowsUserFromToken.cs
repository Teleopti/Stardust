using System.Configuration;
using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.WcfService.Factory;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
    public class WindowsUserFromToken
    {
        private PersonContainer _personContainer;
        private readonly PersonCache _personCache = new PersonCache();
        private CustomWindowsSecurityToken _customWindowsSecurityToken;

        private bool TryGetPersonFromStore()
        {
            //Genomför inloggning. Kasta exception vid fel.
	        var authQuerier = new AuthenticationQuerier(new TenantServerConfiguration(ConfigurationManager.AppSettings["TenantServer"]),
			        new PostHttpRequest(), new NewtonsoftJsonSerializer(),
			        new AuthenticationQuerierResultConverter(new DataSourceConfigDecryption(),
				        () => DataSourceForTenantServiceLocator.DataSourceForTenant, new LoadUserUnauthorized()));

					var result = authQuerier.TryLogon(new IdentityLogonClientModel { Identity = new WebWindowsUserProvider().Identity()}, string.Empty);
            if (result.Success)
            {
                //Spara person till cache.
	            _personContainer = new PersonContainer(result.Person, _customWindowsSecurityToken.WindowsIdentity.Name,
		            null, _customWindowsSecurityToken.DataSource, result.TenantPassword);
                _personCache.Add(_personContainer);
                return true;
            }
            return false;
        }

        private bool TryGetPersonFromCache()
        {
            //Hämta person from cache
            _personContainer = _personCache.Get(_customWindowsSecurityToken.DataSource, _customWindowsSecurityToken.WindowsIdentity.Name);
            return _personContainer != null;
        }

        public void SetPersonFromToken(CustomWindowsSecurityToken customWindowsSecurityToken)
        {
            _customWindowsSecurityToken = customWindowsSecurityToken;
            
            if (TryGetPersonFromCache())
            {
                return;
            }
            if (TryGetPersonFromStore())
            {
                return;
            }
            throw new FaultException(string.Format(CultureInfo.InvariantCulture, "The user with user name {0} could not be found.", _customWindowsSecurityToken.WindowsIdentity.Name));
        }

        public PersonContainer PersonContainer { get { return _personContainer; } }
    }
}