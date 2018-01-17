using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestAvailability : IOvertimeRequestAvailability
	{
		private readonly ILicenseAvailability _licenseAvailability;
		private readonly IAuthorization _authorization;

		public OvertimeRequestAvailability(ILicenseAvailability licenseAvailability, IAuthorization authorization)
		{
			_licenseAvailability = licenseAvailability;
			_authorization = authorization;
		}

		public bool IsEnabled()
		{
			var hasPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb);
			return isLicenseEnabled() && hasPermission;
		}

		private bool isLicenseEnabled()
		{
			return _licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiWfmOvertimeRequests);
		}
	}
}