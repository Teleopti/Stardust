using System;
using System.Configuration;
using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.WcfHost.Service.Factory;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
    public class ApplicationUserFromToken
    {
        private PersonContainer _personContainer;
        private readonly PersonCache _personCache = new PersonCache();
        private CustomUserNameSecurityToken _customUserNameSecurityToken;
        private IDataSource _dataSource;
	    private readonly Guid CustomPersonId = SystemUser.Id;

        private bool tryGetPersonFromStore()
        {
			  
            var result = logOnSystem();
            if (result.Success)
            {
	            _personContainer = new PersonContainer(result.Person, _customUserNameSecurityToken.UserName,
		            _customUserNameSecurityToken.Password, _customUserNameSecurityToken.DataSource, result.TenantPassword);
                _personCache.Add(_personContainer);
                return true;
            }
            return false;
        }

	    private AuthenticationQuerierResult logOnSystem()
	    {
			if (attemptSystemUserLogOn(out var systemUser)) 
					return new AuthenticationQuerierResult {Success = true, Person = systemUser};

		    var authQuerier = new AuthenticationTenantClient(new TenantServerConfiguration(ConfigurationManager.AppSettings["TenantServer"]),
				    new PostHttpRequest(), NewtonsoftJsonSerializer.Make(),
				    new AuthenticationQuerierResultConverter(new DataSourceConfigDecryption(),
					    () => DataSourceForTenantServiceLocator.DataSourceForTenant, new LoadUserUnauthorized()));

		    return authQuerier
			    .TryLogon(
				    new ApplicationLogonClientModel
				    {
					    UserName = _customUserNameSecurityToken.UserName,
					    Password = _customUserNameSecurityToken.Password
				    }, MultiTenancyAuthenticationFactory.UserAgent);
	    }

	    private bool attemptSystemUserLogOn(out IPerson systemUser)
	    {
		    Guid systemUserId;
		    if (Guid.TryParse(_customUserNameSecurityToken.UserName, out systemUserId) && systemUserId == CustomPersonId)
		    {
			    systemUser = new LoadUserUnauthorized().LoadFullPersonInSeperateTransaction(_dataSource.Application, CustomPersonId);
			    return true;
		    }
		    systemUser = null;
		    return false;
	    }

	    private bool tryGetPersonFromCache()
        {
            _personContainer = _personCache.Get(_customUserNameSecurityToken.DataSource, _customUserNameSecurityToken.UserName, _customUserNameSecurityToken.Password);
            return _personContainer != null;
        }

        public void SetPersonFromToken(CustomUserNameSecurityToken customUserNameSecurityToken, IDataSource dataSource)
        {
            _customUserNameSecurityToken = customUserNameSecurityToken;
            _dataSource = dataSource;
            if (tryGetPersonFromCache())
            {
                return;
            }
            if (tryGetPersonFromStore())
            {
                return;
            }
            throw new FaultException(string.Format(CultureInfo.InvariantCulture, "The user with user name {0} could not be found.", _customUserNameSecurityToken.UserName));
        }

        public PersonContainer PersonContainer => _personContainer;
    }
}