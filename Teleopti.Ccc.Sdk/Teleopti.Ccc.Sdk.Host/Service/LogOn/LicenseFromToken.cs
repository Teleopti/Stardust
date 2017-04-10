using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.WcfHost.Service.Factory;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
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