using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public interface ILicenseVerifier
	{
		ILicenseService LoadAndVerifyLicense();
	}
}