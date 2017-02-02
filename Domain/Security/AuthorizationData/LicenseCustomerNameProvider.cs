using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.Security.AuthorizationData
{
	public class LicenseCustomerNameProvider : ILicenseCustomerNameProvider
	{
		private readonly ICurrentDataSource _currentDataSource;

		public LicenseCustomerNameProvider(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public string GetLicenseCustomerName()
		{
			return DefinedLicenseDataFactory.GetLicenseActivator(_currentDataSource?.CurrentName())?.CustomerName;
		}
	}
}