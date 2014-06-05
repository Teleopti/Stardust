using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public class LicenseActivatorProvider : ILicenseActivatorProvider
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICheckLicenseExists _checkLicense;


		public LicenseActivatorProvider(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ICheckLicenseExists checkLicense)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_checkLicense = checkLicense;
		}

		public ILicenseActivator Current()
		{
			var dataSource = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().Name;
			_checkLicense.Check(dataSource);
			return DefinedLicenseDataFactory.GetLicenseActivator(dataSource);
		}
	}
}