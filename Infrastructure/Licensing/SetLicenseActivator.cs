﻿using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public class SetLicenseActivator : ISetLicenseActivator
	{
		private readonly ILicenseVerifierFactory _licenseVerifierFactory;
		private readonly ILog _logger;

		public SetLicenseActivator(ILog logger, ILicenseVerifierFactory licenseVerifierFactory)
		{
			_logger = logger;
			_licenseVerifierFactory = licenseVerifierFactory;
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
					LicenseProvider.ProvideLicenseActivator(unitOfWorkFactory.Name, licenseService);
				}
			}
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