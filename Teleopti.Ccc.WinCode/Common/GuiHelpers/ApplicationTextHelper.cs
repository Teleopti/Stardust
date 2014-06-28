using System;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.WinCode.Common.GuiHelpers
{
    /// <summary>
    /// Class that handles common texts in the application.
    /// Could be used for texts that are the same for all languages e.g
    /// Teleopti WFM, Teleopti etc.
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-11-27
    /// </remarks>
    public static class ApplicationTextHelper
    {
	    /// <summary>
	    /// Gets the licensed to customer text.
	    /// </summary>
	    /// <value>The licensed to customer text.</value>
	    /// <remarks>
	    /// Created by: zoet
	    /// Created date: 2008-11-27
	    /// </remarks>
	    public static string LicensedToCustomerText(string translatedText)
	    {
		    return String.Concat(translatedText, " ",
			    DefinedLicenseDataFactory.GetLicenseActivator(
				    ((ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity).DataSource.DataSourceName).CustomerName);
	    }

	    public static string LoggedOnUserText(string translatedText)
        {
			return String.Concat(translatedText, " ", ((IUnsafePerson)TeleoptiPrincipal.Current).Person.Name); 
		}

    }
}
