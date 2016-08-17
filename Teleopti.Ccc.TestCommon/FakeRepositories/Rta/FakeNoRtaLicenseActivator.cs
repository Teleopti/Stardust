using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeNoRtaLicenseActivator : FakeLicenseActivator
	{
		public FakeNoRtaLicenseActivator()
		{
			DefinedLicenseDataFactory.CreateDefinedLicenseOptions()
				.Where(licenseOption =>
					licenseOption.LicenseOptionPath != DefinedLicenseOptionPaths.TeleoptiCccRealTimeAdherence)
				.Select(licenseOption => licenseOption.LicenseOptionPath)
				.ForEach(EnabledLicenseOptionPaths.Add);
		}
	}
}