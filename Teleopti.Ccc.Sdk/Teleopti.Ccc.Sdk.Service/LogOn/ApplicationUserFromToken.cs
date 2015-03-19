using System;
using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
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
            if (result.Successful)
            {
                _personContainer = new PersonContainer(result.Person)
                                       {
                                           DataSource = _customUserNameSecurityToken.DataSource,
                                           Password = _customUserNameSecurityToken.Password,
                                           UserName = _customUserNameSecurityToken.UserName
                                       };
                _personCache.Add(_personContainer);
                return true;
            }
            return false;
        }

	    private AuthenticationResult logOnSystem()
	    {
		    if (attemptSuperUserLogOn()) return new AuthenticationResult {Successful = true, Person = _dataSourceContainer.User};

		    return _dataSourceContainer.LogOn(_customUserNameSecurityToken.UserName, _customUserNameSecurityToken.Password);
	    }

	    private bool attemptSuperUserLogOn()
	    {
		    Guid systemUser;
		    if (!Guid.TryParse(_customUserNameSecurityToken.UserName, out systemUser) || systemUser != CustomPersonId) return false;
		    using (var unitOfWork = _dataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
		    {
			    var personRep = new PersonRepository(unitOfWork);
			    _dataSourceContainer.SetUser(personRep.LoadPersonAndPermissions(CustomPersonId));
		    }
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