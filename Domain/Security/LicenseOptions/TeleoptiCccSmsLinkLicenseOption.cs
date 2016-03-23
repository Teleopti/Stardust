using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class TeleoptiCccSmsLinkLicenseOption : LicenseOption
	{
		public TeleoptiCccSmsLinkLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccSmsLink, DefinedLicenseOptionNames.TeleoptiCccSmsLink)
		{
		}

		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			EnabledApplicationFunctions.Clear();
		}
	}
}