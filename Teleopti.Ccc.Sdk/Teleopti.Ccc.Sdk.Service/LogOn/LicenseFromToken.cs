using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.WcfService.Factory;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
	public class LicenseFromToken
	{
		public void SetLicense(IDataSourceContainer dataSourceContainer,string tenant)
		{
			var factory = new LicenseFactory(new LicenseCache());
			factory.VerifyLicense(dataSourceContainer.DataSource.Application, tenant);
		}
	}
}