using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeLicenseActivatorProvider : ILicenseActivatorProvider
	{
		private readonly ILicenseActivator _licenseActivator;

		public FakeLicenseActivatorProvider() : this(null)
		{
		}

		public FakeLicenseActivatorProvider(ILicenseActivator licenseActivator)
		{
			_licenseActivator = licenseActivator ?? new FakeLicenseActivator();
		}

		public ILicenseActivator Current()
		{
			return _licenseActivator;
		}
	}
}