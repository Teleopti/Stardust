using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestAvailability : IOvertimeRequestAvailability
	{
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IAuthorization _authorization;

		public OvertimeRequestAvailability(ICurrentDataSource currentDataSource, IAuthorization authorization)
		{
			_currentDataSource = currentDataSource;
			_authorization = authorization;
		}

		public bool IsEnabled()
		{
			var hasPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb);
			return IsLicenseEnabled() && hasPermission;
		}

		public bool IsLicenseEnabled()
		{
			var currentName = _currentDataSource.CurrentName();
			var isLicenseAvailible = DefinedLicenseDataFactory.HasLicense(currentName) &&
									DefinedLicenseDataFactory.GetLicenseActivator(currentName).EnabledLicenseOptionPaths.Contains(
									DefinedLicenseOptionPaths.TeleoptiWfmOvertimeRequests);
			return isLicenseAvailible;
		}
	}
}