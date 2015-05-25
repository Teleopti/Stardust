using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.WcfService.Factory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
	public class LicenseFromToken
	{
		public void SetLicense(IDataSource dataSource,string tenant)
		{
			var factory = new LicenseFactory(new LicenseCache());
			factory.VerifyLicense(dataSource.Application, tenant);
		}
	}
}