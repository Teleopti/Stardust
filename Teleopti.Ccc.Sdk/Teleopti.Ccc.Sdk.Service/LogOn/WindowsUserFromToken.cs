using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
    public class WindowsUserFromToken
    {
        private PersonContainer _personContainer;
        private readonly PersonCache _personCache = new PersonCache();
        private CustomWindowsSecurityToken _customWindowsSecurityToken;
        //private IDataSourceContainer _dataSourceContainer;

        private bool TryGetPersonFromStore()
        {
            //Genomför inloggning. Kasta exception vid fel.
            //var result = _dataSourceContainer.LogOn(_customWindowsSecurityToken.WindowsIdentity.Name);
	        var result = TenancyLogonFactory.MultiTenancyWindowsLogon()
		        .Logon(new LogonModel(), StateHolderReader.Instance.StateReader.ApplicationScopeData, "");
            if (result.Successful)
            {
                //Spara person till cache.
                _personContainer = new PersonContainer(result.Person)
                                       {
                                           DataSource = _customWindowsSecurityToken.DataSource,
                                           UserName = _customWindowsSecurityToken.WindowsIdentity.Name
                                       };
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