﻿using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public class InitializeLicenseServiceForTenant : IInitializeLicenseServiceForTenant
	{
		private readonly ILicenseVerifierFactory _licenseVerifierFactory;
		private readonly ILicensedFunctionsProvider _licensedFunctionsProvider;
		private readonly ILog _logger;

		public InitializeLicenseServiceForTenant(ILog logger, ILicenseVerifierFactory licenseVerifierFactory, ILicensedFunctionsProvider licensedFunctionsProvider)
		{
			_logger = logger;
			_licenseVerifierFactory = licenseVerifierFactory;
			_licensedFunctionsProvider = licensedFunctionsProvider;
		}

		public bool TryInitialize(IDataSource dataSource)
		{
			//don't really know what this does - extracted from web startup
			if (dataSource.Application == null)
				return false;
			var licenseVerifier = _licenseVerifierFactory.Create(this, dataSource.Application);
			using (dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var licenseService = licenseVerifier.LoadAndVerifyLicense();
				if (licenseService == null) 
					return false;
				clearCashedLicenseData(dataSource.DataSourceName);
				LicenseProvider.ProvideLicenseActivator(dataSource.DataSourceName, licenseService);
				return true;
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