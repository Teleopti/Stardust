using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
    public class ApplicationUserFromToken
    {
        private PersonContainer _personContainer;
        private readonly PersonCache _personCache = new PersonCache();
        private CustomUserNameSecurityToken _customUserNameSecurityToken;
        private IDataSourceContainer _dataSourceContainer;

        private bool TryGetPersonFromStore()
        {
            //Genomför inloggning. Kasta exception vid fel.
            var result = _dataSourceContainer.LogOn(_customUserNameSecurityToken.UserName, _customUserNameSecurityToken.Password);
            if (result.Successful)
            {
                //Spara person till cache.
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

        private bool TryGetPersonFromCache()
        {
            //Hämta person from cache
            _personContainer = _personCache.Get(_customUserNameSecurityToken.DataSource, _customUserNameSecurityToken.UserName, _customUserNameSecurityToken.Password);
            return _personContainer != null;
        }

        public void SetPersonFromToken(CustomUserNameSecurityToken customUserNameSecurityToken, IDataSourceContainer dataSourceContainer)
        {
            _customUserNameSecurityToken = customUserNameSecurityToken;
            _dataSourceContainer = dataSourceContainer;
            if (TryGetPersonFromCache())
            {
                return;
            }
            if (TryGetPersonFromStore())
            {
                return;
            }
            throw new FaultException(string.Format(CultureInfo.InvariantCulture, "The user with user name {0} could not be found.", _customUserNameSecurityToken.UserName));
        }

        public PersonContainer PersonContainer { get { return _personContainer; } }
    }
}