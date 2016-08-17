using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeNoRtaLicenseActivatorProvider : ILicenseActivatorProvider
	{
		public ILicenseActivator Current()
		{
			return new FakeNoRtaLicenseActivator();
		}
	}
}