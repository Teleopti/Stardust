using System;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.WinCode.Common.GuiHelpers
{
    public class ApplicationTextHelper
    {
	    public string LicensedToCustomerText(string translatedText)
	    {
		    return String.Concat(translatedText, " ",
			    DefinedLicenseDataFactory.GetLicenseActivator(
				    ((ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity).DataSource.DataSourceName).CustomerName);
	    }

	    public string LoggedOnUserText(string translatedText)
        {
			return String.Concat(translatedText, " ", ((IUnsafePerson)TeleoptiPrincipal.Current).Person.Name); 
		}

    }
}
