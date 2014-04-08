using Teleopti.Ccc.Secrets.Licensing;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public interface ILicenseVerifier
	{
		ILicenseService LoadAndVerifyLicense();
	}
}