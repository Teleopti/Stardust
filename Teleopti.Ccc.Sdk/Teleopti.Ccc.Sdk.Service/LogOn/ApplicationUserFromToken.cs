using System;
using System.Configuration;
using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
    public class ApplicationUserFromToken
    {
        private PersonContainer _personContainer;
        private readonly PersonCache _personCache = new PersonCache();
        private CustomUserNameSecurityToken _customUserNameSecurityToken;
        private IDataSourceContainer _dataSourceContainer;
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
		    if (attemptSuperUserLogOn()) 
					return new AuthenticationQuerierResult {Success = true, Person = _dataSourceContainer.User};

		    var authQuerier = new AuthenticationQuerier(new TenantServerConfiguration(ConfigurationManager.AppSettings["TenantServer"]),
				    new PostHttpRequest(), new NewtonsoftJsonSerializer(),
				    new AuthenticationQuerierResultConverter(new NhibConfigDecryption(),
					    () => StateHolder.Instance.StateReader.ApplicationScopeData, new LoadUserUnauthorized()));

		    return authQuerier
			    .TryLogon(
				    new ApplicationLogonClientModel
				    {
					    UserName = _customUserNameSecurityToken.UserName,
					    Password = _customUserNameSecurityToken.Password
				    }, string.Empty);
	    }

	    private bool attemptSuperUserLogOn()
	    {
		    Guid systemUser;
		    if (!Guid.TryParse(_customUserNameSecurityToken.UserName, out systemUser) || systemUser != CustomPersonId) return false;
				_dataSourceContainer.SetUser(new LoadUserUnauthorized().LoadFullPersonInSeperateTransaction(_dataSourceContainer.DataSource.Application, CustomPersonId));
		    return true;
	    }

	    private bool tryGetPersonFromCache()
        {
            _personContainer = _personCache.Get(_customUserNameSecurityToken.DataSource, _customUserNameSecurityToken.UserName, _customUserNameSecurityToken.Password);
            return _personContainer != null;
        }

        public void SetPersonFromToken(CustomUserNameSecurityToken customUserNameSecurityToken, IDataSourceContainer dataSourceContainer)
        {
            _customUserNameSecurityToken = customUserNameSecurityToken;
            _dataSourceContainer = dataSourceContainer;
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