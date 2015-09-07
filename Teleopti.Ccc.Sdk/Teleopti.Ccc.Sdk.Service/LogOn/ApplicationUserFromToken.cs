using System;
using System.Configuration;
using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
    public class ApplicationUserFromToken
    {
        private PersonContainer _personContainer;
        private readonly PersonCache _personCache = new PersonCache();
        private CustomUserNameSecurityToken _customUserNameSecurityToken;
        private IDataSource _dataSource;
	    private readonly Guid CustomPersonId = SuperUser.Id_AvoidUsing_This;

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
		    IPerson superUser;
		    if (attemptSuperUserLogOn(out superUser)) 
					return new AuthenticationQuerierResult {Success = true, Person = superUser};

		    var authQuerier = new AuthenticationQuerier(new TenantServerConfiguration(ConfigurationManager.AppSettings["TenantServer"]),
				    new PostHttpRequest(), new NewtonsoftJsonSerializer(),
				    new AuthenticationQuerierResultConverter(new DataSourceConfigDecryption(),
					    () => StateHolder.Instance.StateReader.ApplicationScopeData.DataSourceForTenant, new LoadUserUnauthorized()));

		    return authQuerier
			    .TryLogon(
				    new ApplicationLogonClientModel
				    {
					    UserName = _customUserNameSecurityToken.UserName,
					    Password = _customUserNameSecurityToken.Password
				    }, string.Empty);
	    }

	    private bool attemptSuperUserLogOn(out IPerson superUser)
	    {
		    Guid systemUser;
		    if (Guid.TryParse(_customUserNameSecurityToken.UserName, out systemUser) && systemUser == CustomPersonId)
		    {
			    superUser = new LoadUserUnauthorized().LoadFullPersonInSeperateTransaction(_dataSource.Application,
				    CustomPersonId);
			    return true;
		    }
		    superUser = null;
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

        public PersonContainer PersonContainer { get { return _personContainer; } }
    }
}