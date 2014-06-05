using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public class CheckLicenseExists : ICheckLicenseExists
	{
		public const string ErrorMessageIfNoLicenseAtAll = "Missing datasource (no *.hbm.xml file available)!";
		public const string ErrorMessageIfNoLicenseForDataSource = "No license for datasource {0}!";

		public void Check(string dataSource)
		{
			if (!DefinedLicenseDataFactory.HasAnyLicense)
				throw new DataSourceException(ErrorMessageIfNoLicenseAtAll);

			if (!DefinedLicenseDataFactory.HasLicense(dataSource))
				throw new DataSourceException(string.Format(ErrorMessageIfNoLicenseForDataSource, dataSource));
		}
	}
}