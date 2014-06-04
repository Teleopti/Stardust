using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public class LicenseActivatorProvider : ILicenseActivatorProvider
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		public const string ErrorMessageIfNoLicenseAtAll = "Missing datasource (no *.hbm.xml file available)!";
		public const string ErrorMessageIfNoLicenseForDataSource = "No license for datasource {0}!";

		public LicenseActivatorProvider(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public ILicenseActivator Current()
		{
			if (!DefinedLicenseDataFactory.HasAnyLicense)
				throw new DataSourceException(ErrorMessageIfNoLicenseAtAll);

			var dataSource = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().Name;
			if (!DefinedLicenseDataFactory.HasLicense(dataSource))
				throw new DataSourceException(string.Format(ErrorMessageIfNoLicenseForDataSource, dataSource));
			return DefinedLicenseDataFactory.GetLicenseActivator(dataSource);
		}
	}
}