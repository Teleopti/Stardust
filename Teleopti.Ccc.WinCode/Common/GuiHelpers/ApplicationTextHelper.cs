using System;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.WinCode.Common.GuiHelpers
{
    public class ApplicationTextHelper
    {
	    public static string LicensedToCustomerText => String.Concat(" ",
		    DefinedLicenseDataFactory.GetLicenseActivator(
			    ((ITeleoptiIdentity) TeleoptiPrincipal.CurrentPrincipal.Identity).DataSource.DataSourceName).CustomerName);

	    public static string LoggedOnUserText => String.Concat(" ", TeleoptiPrincipal.CurrentPrincipal.Person().Name);
    }
}
