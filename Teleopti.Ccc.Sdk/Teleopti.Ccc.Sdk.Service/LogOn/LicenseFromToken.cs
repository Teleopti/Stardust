using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.WcfService.Factory;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
    public class LicenseFromToken
    {
				[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dataSourceContainer")]
				public void SetLicense(IDataSourceContainer dataSourceContainer)
        {
            LicenseFactory factory = new LicenseFactory(new LicenseCache(), UnitOfWorkFactory.CurrentUnitOfWorkFactory());
            factory.VerifyLicense();
        }
    }
}