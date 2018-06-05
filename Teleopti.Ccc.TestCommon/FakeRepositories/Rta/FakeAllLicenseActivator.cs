using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAllLicenseActivator : FakeLicenseActivator
	{
		public FakeAllLicenseActivator()
		{
			DefinedLicenseDataFactory.CreateDefinedLicenseOptions()
				.Select(licenseOption => licenseOption.LicenseOptionPath)
				.ForEach(EnabledLicenseOptionPaths.Add);
		}
	}
}