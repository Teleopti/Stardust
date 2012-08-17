using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccSmsLinkLicenseOption : LicenseOption
	{
		public TeleoptiCccSmsLinkLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccSmsLink, DefinedLicenseOptionNames.TeleoptiCccSmsLink)
		{
		}
	}
}