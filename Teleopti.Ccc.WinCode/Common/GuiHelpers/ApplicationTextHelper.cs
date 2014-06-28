using System;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.WinCode.Common.GuiHelpers
{
    public class ApplicationTextHelper
    {
	    public string LicensedToCustomerText
	    {
            get
            {
	            return String.Concat(UserTexts.Resources.LicensedToColon, " ",
		            DefinedLicenseDataFactory.GetLicenseActivator(
			            ((ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity).DataSource.DataSourceName).CustomerName);
            }
        }

	    public string LoggedOnUserText
        {
            get {
                return String.Concat(UserTexts.Resources.LoggedOnUserColon, " ", ((IUnsafePerson)TeleoptiPrincipal.Current).Person.Name); }
        }
    }
}
