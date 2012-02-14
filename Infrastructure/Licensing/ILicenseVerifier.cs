using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public interface ILicenseVerifier
	{
		ILicenseService LoadAndVerifyLicense();
	}
}