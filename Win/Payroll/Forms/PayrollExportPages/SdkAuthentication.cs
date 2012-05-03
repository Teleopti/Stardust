using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Payroll.Forms.PayrollExportPages
{
    public class SdkAuthentication : ISdkAuthentication
    {
        public void SetSdkAuthenticationHeader()
        {
            var principal = TeleoptiPrincipal.Current;
            var identity = ((ITeleoptiIdentity)principal.Identity);
            AuthenticationMessageHeader.BusinessUnit = identity.BusinessUnit.Id.GetValueOrDefault();
            AuthenticationMessageHeader.DataSource = identity.DataSource.Application.Name;
            if (((IUnsafePerson)principal).Person.ApplicationAuthenticationInfo != null)
            {
                AuthenticationMessageHeader.UserName = ((IUnsafePerson)principal).Person.ApplicationAuthenticationInfo.ApplicationLogOnName;
                AuthenticationMessageHeader.Password = ((IUnsafePerson)principal).Person.ApplicationAuthenticationInfo.Password;
            }
            
            AuthenticationMessageHeader.UseWindowsIdentity =
                StateHolderReader.Instance.StateReader.SessionScopeData.AuthenticationTypeOption ==
                AuthenticationTypeOption.Windows;
        }
    }

    public interface ISdkAuthentication
    {
        void SetSdkAuthenticationHeader();
    }
}