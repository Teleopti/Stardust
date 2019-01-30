using System;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers
{
    public class ApplicationTextHelper
    {
	    public static string LicensedToCustomerText
	    {
            get
            {
	            return String.Concat(" ",
		            DefinedLicenseDataFactory.GetLicenseActivator(
			            ((ITeleoptiIdentity) TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity).DataSource.DataSourceName).CustomerName);
            }
        }

	    public static string LoggedOnUserText
	    {
		    get
		    {
			    return String.Concat(" ", ((ITeleoptiPrincipalWithUnsafePerson) TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal).UnsafePerson().Name);
		    }
	    }
    }
}
