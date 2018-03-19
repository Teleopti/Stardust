using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public class SetLicenseActivator : ISetLicenseActivator
	{
		private readonly ILicenseVerifierFactory _licenseVerifierFactory;
		private readonly ILicensedFunctionsProvider _licensedFunctionsProvider;
		private readonly ILog _logger;

		public SetLicenseActivator(ILog logger, ILicenseVerifierFactory licenseVerifierFactory, ILicensedFunctionsProvider licensedFunctionsProvider)
		{
			_logger = logger;
			_licenseVerifierFactory = licenseVerifierFactory;
			_licensedFunctionsProvider = licensedFunctionsProvider;
		}

		public void Execute(IUnitOfWorkFactory unitOfWorkFactory)
		{
			//don't really know what this does - extracted from web startup
			var licenseVerifier = _licenseVerifierFactory.Create(this, unitOfWorkFactory);
			using (unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var licenseService = licenseVerifier.LoadAndVerifyLicense();
				if (licenseService != null)
				{
					clearCashedLicenseData(unitOfWorkFactory.Name);
					LicenseProvider.ProvideLicenseActivator(unitOfWorkFactory.Name, licenseService);
				}
			}
		}

		private void clearCashedLicenseData(string tenant)
		{
			LicenseSchema.ClearActiveLicenseSchemas(tenant);
			DefinedLicenseDataFactory.ClearLicenseActivators(tenant);
			_licensedFunctionsProvider.ClearLicensedFunctions(tenant);
		}

		public void Warning(string warning)
		{
			Warning(warning, "");
		}

		public void Warning(string warning, string caption)
		{
			_logger.Warn(warning);
		}

		public void Error(string error)
		{
			_logger.Error(error);
		}
	}
}