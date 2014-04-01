using System;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	[CLSCompliant(false)]
	public interface ILicenseVerifier
	{
		[CLSCompliant(false)]
		ILicenseService LoadAndVerifyLicense();
	}
}