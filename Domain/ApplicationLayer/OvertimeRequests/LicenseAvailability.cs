using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class LicenseAvailability: ILicenseAvailability
	{
		private readonly ICurrentDataSource _currentDataSource;

		public LicenseAvailability(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public bool IsLicenseEnabled(string licensePath)
		{
			var currentName = _currentDataSource.CurrentName();
			var isLicenseAvailible = DefinedLicenseDataFactory.HasLicense(currentName) &&
									 DefinedLicenseDataFactory.GetLicenseActivator(currentName).EnabledLicenseOptionPaths.Contains(licensePath);
			return isLicenseAvailible;
		}
	}
}